using Dapper;
using LibX4.FileSystem;
using LibX4.Xml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;
using X4_DataExporterWPF.Internal;

namespace X4_DataExporterWPF.Export;

/// <summary>
/// 艦船のドック情報を抽出する
/// </summary>
class ShipHangerExporter : IExporter
{
    /// <summary>
    /// catファイルオブジェクト
    /// </summary>
    private readonly IIndexResolver _CatFile;


    /// <summary>
    /// ウェア情報xml
    /// </summary>
    private readonly XDocument _WaresXml;


    /// <summary>
    /// ドックエリア名をキーにしたサイズIDとドック数のディクショナリ
    /// </summary>
    private readonly Dictionary<string, IReadOnlyDictionary<string, int>> _DockAreaDict = new();


    /// <summary>
    /// ドッキングベイ名をキーにしたサイズIDのディクショナリ
    /// </summary>
    private readonly Dictionary<string, string?> _DockingBaySizeDict = new();


    /// <summary>
    /// ドッキングベイ名をキーにしたサイズIDと機体格納量のディクショナリ
    /// </summary>
    private readonly Dictionary<string, (string, int)?> _DockingBayDict = new();




    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="catFile">catファイルオブジェクト</param>
    /// <param name="waresXml">ウェア情報xml</param>
    public ShipHangerExporter(IIndexResolver catFile, XDocument waresXml)
    {
        _CatFile = catFile;
        _WaresXml = waresXml;
    }


    /// <inheritdoc/>
    public async Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            await connection.ExecuteAsync(@"
CREATE TABLE IF NOT EXISTS ShipHanger
(
    ShipID      TEXT    NOT NULL,
    SizeID      TEXT    NOT NULL,
    Count       INTEGER NOT NULL,
    Capacity    INTEGER NOT NULL,
    PRIMARY KEY (ShipID, SizeID),
    FOREIGN KEY (ShipID)    REFERENCES Ship(ShipID),
    FOREIGN KEY (SizeID)    REFERENCES Size(SizeID)
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecordsAsync(progress, cancellationToken);

            await connection.ExecuteAsync(@"INSERT INTO ShipHanger(ShipID, SizeID, Count, Capacity) VALUES (@ShipID, @SizeID, @Count, @Capacity)", items);
        }
    }


    /// <summary>
    /// レコード抽出
    /// </summary>
    private async IAsyncEnumerable<ShipHanger> GetRecordsAsync(IProgress<(int currentStep, int maxSteps)> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var maxSteps = (int)(double)_WaresXml.Root!.XPathEvaluate("count(ware[contains(@tags, 'ship')])");
        var currentStep = 0;


        foreach (var ship in _WaresXml.Root!.XPathSelectElements("ware[contains(@tags, 'ship')]"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report((currentStep++, maxSteps));

            var shipID = ship.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(shipID)) continue;

            XDocument? shipMacroXml;
            {
                var macroName = ship.XPathSelectElement("component")?.Attribute("ref")?.Value;
                if (string.IsNullOrEmpty(macroName)) continue;

                shipMacroXml = await _CatFile.OpenIndexXmlAsync("index/macros.xml", macroName, cancellationToken);
                if (shipMacroXml?.Root is null) continue;
            }

            var componentName = shipMacroXml.Root.XPathSelectElement("macro/component")?.Attribute("ref")?.Value;
            if (string.IsNullOrEmpty(componentName)) continue;

            var componentXml = await _CatFile.OpenIndexXmlAsync("index/components.xml", componentName, cancellationToken);
            if (componentXml?.Root is null) continue;

            // 集計用辞書
            var aggregateDict = new Dictionary<string, HangerInfo>();

            // ドックエリア数ループ
            foreach (var conName in componentXml.Root.XPathSelectElements("component/connections/connection[contains(@tags, 'dockarea')]").Select(x => x.Attribute("name")?.Value).OfType<string>())
            {
                foreach (var (size, count) in await CountDockAreaAsync(shipMacroXml, conName, cancellationToken))
                {
                    if (!aggregateDict.ContainsKey(size))
                    {
                        aggregateDict.Add(size, new HangerInfo());
                    }

                    aggregateDict[size].Count += count;
                }
            }


            // ドッキングベイ数ループ
            foreach (var conName in componentXml.Root.XPathSelectElements("component/connections/connection[contains(@tags, 'dockingbay')]").Select(x => x.Attribute("name")?.Value).OfType<string>())
            {
                var dockingBay = await GetDockingBayCapacityAsync(shipMacroXml, conName, cancellationToken);
                if (dockingBay is null) continue;

                if (!aggregateDict.ContainsKey(dockingBay.Value.Item1))
                {
                    aggregateDict.Add(dockingBay.Value.Item1, new HangerInfo());
                }

                aggregateDict[dockingBay.Value.Item1].Capacity += dockingBay.Value.Item2;
            }


            foreach (var (size, info) in aggregateDict.Where(x => 0 < x.Value.Capacity || 0 < x.Value.Count))
            {
                yield return new ShipHanger(shipID, size, info.Count, info.Capacity);
            }
        }

        progress.Report((currentStep++, maxSteps));
    }


    /// <summary>
    /// 指定したドッキングベイの機体格納数を取得する
    /// </summary>
    /// <param name="shipMacroXml"></param>
    /// <param name="dockingBayName"></param>
    /// <returns></returns>
    private async Task<(string, int)?> GetDockingBayCapacityAsync(XDocument shipMacroXml, string dockingBayName, CancellationToken cancellationToken)
    {
        var macroName = shipMacroXml.Root?.XPathSelectElement($"macro/connections/connection[@ref='{dockingBayName}']/macro")?.Attribute("ref")?.Value ?? "";
        if (string.IsNullOrEmpty(macroName))
        {
            return null;
        }

        // 登録済みにあればxmlを見ないで機体格納数を返す
        if (_DockingBayDict.TryGetValue(macroName, out var registerdCapacity))
        {
            return registerdCapacity;
        }

        var dockingBayXml = await _CatFile.OpenIndexXmlAsync("index/macros.xml", macroName, cancellationToken);
        if (dockingBayXml is null)
        {
            _DockingBayDict.TryAdd(macroName, null);
            return null;
        }

        var capacity = dockingBayXml.Root?.XPathSelectElement("macro/properties/dock")?.Attribute("capacity")?.GetInt() ?? 0;
        var tags = dockingBayXml.Root?.XPathSelectElement("macro/properties/docksize")?.Attribute("tags")?.Value ?? "";
        var size = DockSize2SizeID(tags);

        // 容量が1未満 又は 無効なドックサイズの場合、空の要素を登録
        if (capacity < 1 || string.IsNullOrEmpty(size))
        {
            _DockingBayDict.TryAdd(macroName, null);
            return null;
        }

        _DockingBayDict.Add(macroName, (size, capacity));

        return (size, capacity);
    }




    /// <summary>
    /// 指定したドック名に対応するドック数をカウントする
    /// </summary>
    /// <param name="shipMacroXml">艦船マクロ</param>
    /// <param name="dockName">ドック名</param>
    /// <returns></returns>
    private async Task<IReadOnlyDictionary<string, int>> CountDockAreaAsync(XDocument shipMacroXml, string dockName, CancellationToken cancellationToken)
    {
        var macroName = shipMacroXml.Root?.XPathSelectElement($"macro/connections/connection[@ref='{dockName}']/macro")?.Attribute("ref")?.Value ?? "";
        if (string.IsNullOrEmpty(macroName))
        {
            return new Dictionary<string, int>();
        }

        // 登録済みにあればxmlを見ないでドック数を返す
        if (_DockAreaDict.TryGetValue(macroName, out var registeredDict))
        {
            return registeredDict;
        }



        var dockMacroXml = await _CatFile.OpenIndexXmlAsync("index/macros.xml", macroName, cancellationToken);
        if (dockMacroXml is null)
        {
            var ret = new Dictionary<string, int>();
            _DockAreaDict.TryAdd(macroName, ret);
            return ret;
        }

        // 装備集計用辞書
        var sizeDict = new Dictionary<string, int>()
        {
            { "extrasmall", 0 },
            { "small",      0 },
            { "medium",     0 },
            { "large",      0 },
            { "extralarge", 0 }
        };

        foreach (var dockingBayName in dockMacroXml.Root?.XPathSelectElements("macro/connections/connection/macro").Attributes("ref").Select(x => x.Value) ?? Enumerable.Empty<string>())
        {
            var size = await GetDockingBaysSizeAsync(dockingBayName, cancellationToken) ?? "";
            if (sizeDict.ContainsKey(size))
            {
                sizeDict[size]++;
            }
        }


        {
            var ret = sizeDict.Where(x => 0 < x.Value).ToDictionary(x => x.Key, x => x.Value);
            _DockAreaDict.Add(macroName, ret);
            return ret;
        }
    }


    /// <summary>
    /// ドッキングベイのサイズを取得する
    /// </summary>
    /// <param name="dockingBayMacroName">ドッキングベイのマクロ名</param>
    /// <returns>ドッキングベイのサイズID</returns>
    private async Task<string?> GetDockingBaysSizeAsync(string dockingBayMacroName, CancellationToken cancellationToken)
    {
        // メモに残っていればそれを返す
        if (_DockingBaySizeDict.TryGetValue(dockingBayMacroName, out var size))
        {
            return size;
        }

        var dockingMacroXml = await _CatFile.OpenIndexXmlAsync("index/macros.xml", dockingBayMacroName, cancellationToken);
        if (dockingMacroXml is null)
        {
            _DockingBaySizeDict.Add(dockingBayMacroName, null);
            return null;
        }

        var tag = dockingMacroXml.Root?.XPathSelectElement("macro/properties/docksize")?.Attribute("tags")?.Value ?? "";

        size = DockSize2SizeID(tag);

        _DockingBaySizeDict.Add(dockingBayMacroName, size);
        return size;
    }



    /// <summary>
    /// ドックサイズをサイズIDに変換する
    /// </summary>
    /// <param name="dockSize">ドックサイズ</param>
    /// <returns>サイズID</returns>
    private string? DockSize2SizeID(string dockSize)
    {
        return dockSize switch
        {
            "dock_xs" => "extrasmall",
            "dock_s" => "small",
            "dock_m" => "medium",
            "dock_l" => "large",
            "dock_xl" => "extralarge",
            _ => null
        };
    }


    private class HangerInfo
    {
        public int Count;
        public int Capacity;
    }
}

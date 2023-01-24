using Dapper;
using LibX4.FileSystem;
using LibX4.Lang;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;
using X4_DataExporterWPF.Internal;

namespace X4_DataExporterWPF.Export;

/// <summary>
/// 艦船種別抽出用クラス
/// </summary>
public class ShipTypeExporter : IExporter
{
    /// <summary>
    /// catファイルオブジェクト
    /// </summary>
    private readonly CatFile _CatFile;


    /// <summary>
    /// ウェア情報xml
    /// </summary>
    private readonly XDocument _WaresXml;


    /// <summary>
    /// 言語解決用オブジェクト
    /// </summary>
    private readonly ILanguageResolver _Resolver;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="resolver">言語解決用オブジェクト</param>
    public ShipTypeExporter(CatFile catFile, XDocument waresXml, LanguageResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(waresXml.Root);

        _CatFile = catFile;
        _WaresXml = waresXml;
        _Resolver = resolver;
    }


    /// <inheritdoc/>
    public async Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            await connection.ExecuteAsync(@"
CREATE TABLE IF NOT EXISTS ShipType
(
    ShipTypeID  TEXT    NOT NULL PRIMARY KEY,
    Name        TEXT    NOT NULL,
    Description TEXT    NOT NULL
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecordsAsync(progress, cancellationToken);

            await connection.ExecuteAsync("INSERT INTO ShipType (ShipTypeID, Name, Description) VALUES (@ShipTypeID, @Name, @Description)", items);
        }
    }


    private async IAsyncEnumerable<ShipType> GetRecordsAsync(IProgress<(int currentStep, int maxSteps)> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // TODO: 可能ならファイルから抽出する
        var names = new Dictionary<string, (int name, int descr)>
        {
            // 特小
            {"personalvehicle",  (1001, 1002)},         // 個人乗用船
            {"police",           (1011, 1012)},         // 警察船
            {"xsdrone",          (1021, 1022)},         // ドローン
            {"escapepod",        (1031, 1032)},         // 脱出ポッド
            {"lasertower",       (1041, 1042)},         // レーザータワー
            {"distressdrone",    (1051, 1052)},         // ドローン

            // 小型
            {"scout",            (2001, 2002)},         // 偵察機
            {"fighter",          (2011, 2012)},         // 戦闘機
            {"heavyfighter",     (2021, 2022)},         // 重戦闘機
            {"interceptor",      (2031, 2032)},         // 要撃機
            {"courier",          (2041, 2042)},         // 配達船
            {"smalldrone",       (2051, 2052)},         // ドローン

            // 中型
            {"bomber",           (3001, 3002)},         // 爆撃機
            {"frigate",          (3011, 3012)},         // フリゲート
            {"corvette",         (3021, 3022)},         // コルベット
            {"transporter",      (3031, 3032)},         // 輸送船
            {"miner",            (3041, 3042)},         // 採掘船
            {"personnelcarrier", (3051, 3052)},         // 人員輸送船 (IDは仮)
            {"scavenger",        (3061, 3062)},         // 廃品回収船
            {"gunboat",          (5041, 5042)},         // 砲艦
            {"tug",              (5051, 5052)},         // 曳船

            // 大型
            {"destroyer",        (4001, 4002)},         // 駆逐艦
            {"freighter",        (4011, 4012)},         // 貨物船
            {"largeminer",       (4021, 4022)},         // 採掘船
            {"compactor",        (5061, 5062)},         // 圧縮作業船 (IDは仮)

            // 特大型
            {"carrier",          (5001, 5002)},         // 空母
            {"resupplier",       (5011, 5012)},         // 補助艦船
            {"builder",          (5021, 5022)},         // 建築船
            {"battleship",       (5031, 5032)},         // 戦艦
        };

        var maxSteps = (int)(double)_WaresXml.Root!.XPathEvaluate("count(ware[contains(@tags, 'ship')])");
        var currentStep = 0;
        var added = new HashSet<string>();

        foreach (var ship in _WaresXml.Root!.XPathSelectElements("ware[contains(@tags, 'ship')]"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((currentStep++, maxSteps));

            var shipID = ship.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(shipID)) continue;

            var macroName = ship.XPathSelectElement("component")?.Attribute("ref")?.Value;
            if (string.IsNullOrEmpty(macroName)) continue;

            var macroXml = await _CatFile.OpenIndexXmlAsync("index/macros.xml", macroName, cancellationToken);

            var properties = macroXml.Root?.XPathSelectElement("macro/properties");
            if (properties is null) continue;

            var shipTypeID = properties.Element("ship")?.Attribute("type")?.Value ?? "";
            if (string.IsNullOrEmpty(shipTypeID) || added.Contains(shipTypeID)) continue;

            var name = shipTypeID;
            var descr = "";
            if (names.TryGetValue(shipTypeID, out var item))
            {
                name = _Resolver.Resolve($"{{20221, {item.name}}}");
                descr = _Resolver.Resolve($"{{20221, {item.descr}}}");
            }

            yield return new ShipType(shipTypeID, name, descr);
            added.Add(shipTypeID);
        }

        progress?.Report((currentStep++, maxSteps));
    }
}

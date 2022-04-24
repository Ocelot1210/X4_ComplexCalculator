using Dapper;
using LibX4.FileSystem;
using LibX4.Xml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export;

/// <summary>
/// モジュール情報抽出用クラス
/// </summary>
public class ModuleExporter : IExporter
{
    /// <summary>
    /// catファイルオブジェクト
    /// </summary>
    private readonly ICatFile _CatFile;

    /// <summary>
    /// ウェア情報xml
    /// </summary>
    private readonly XDocument _WaresXml;


    /// <summary>
    /// サムネが見つからなかった場合のサムネ
    /// </summary>
    private byte[]? _NotFoundThumb;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="catFile">catファイルオブジェクト</param>
    /// <param name="waresXml">ウェア情報xml</param>
    public ModuleExporter(ICatFile catFile, XDocument waresXml)
    {
        _CatFile = catFile;
        _WaresXml = waresXml;
    }


    /// <summary>
    /// 抽出処理
    /// </summary>
    /// <param name="connection"></param>
    public void Export(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            connection.Execute(@"
CREATE TABLE IF NOT EXISTS Module
(
    ModuleID        TEXT    NOT NULL PRIMARY KEY,
    ModuleTypeID    TEXT    NOT NULL,
    Macro           TEXT    NOT NULL,
    MaxWorkers      INTEGER NOT NULL,
    WorkersCapacity INTEGER NOT NULL,
    NoBlueprint     BOOLEAN NOT NULL,
    Thumbnail       BLOB,
    FOREIGN KEY (ModuleTypeID)  REFERENCES ModuleType(ModuleTypeID)
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecords(progress);

            connection.Execute("INSERT INTO Module (ModuleID, ModuleTypeID, Macro, MaxWorkers, WorkersCapacity, NoBlueprint, Thumbnail) VALUES (@ModuleID, @ModuleTypeID, @Macro, @MaxWorkers, @WorkersCapacity, @NoBlueprint, @Thumbnail)", items);
        }
    }


    /// <summary>
    /// XML から Module データを読み出す
    /// </summary>
    /// <returns>読み出した Module データ</returns>
    internal IEnumerable<Module> GetRecords(IProgress<(int currentStep, int maxSteps)>? progress = null)
    {
        var maxSteps = (int)(double)_WaresXml.Root.XPathEvaluate("count(ware[contains(@tags, 'module')])");
        var currentStep = 0;


        foreach (var module in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'module')]"))
        {
            progress?.Report((currentStep++, maxSteps));


            var moduleID = module.Attribute("id").Value;
            if (string.IsNullOrEmpty(moduleID)) continue;

            var macroName = module.XPathSelectElement("component").Attribute("ref").Value;
            if (string.IsNullOrEmpty(macroName)) continue;

            var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);
            var moduleTypeID = macroXml.Root.XPathSelectElement("macro").Attribute("class").Value;
            if (string.IsNullOrEmpty(moduleTypeID)) continue;

            // 従業員数/最大収容人数取得
            var workForce = macroXml?.Root?.XPathSelectElement("macro/properties/workforce");
            var maxWorkers = workForce?.Attribute("max")?.GetInt() ?? 0;
            var capacity = workForce?.Attribute("capacity")?.GetInt() ?? 0;

            var noBluePrint = module.Attribute("tags").Value.Contains("noblueprint");

            yield return new Module(moduleID, moduleTypeID, macroName, maxWorkers, capacity, noBluePrint, GetThumbnail(macroName));
        }

        progress?.Report((currentStep++, maxSteps));
    }


    /// <summary>
    /// サムネ画像を取得する
    /// </summary>
    /// <param name="macroName">マクロ名</param>
    /// <returns>サムネ画像のバイト配列</returns>
    private byte[]? GetThumbnail(string macroName)
    {
        const string dir = "assets/fx/gui/textures/stationmodules";
        var thumb = Util.DDS2Png(_CatFile, dir, macroName);
        if (thumb is not null)
        {
            return thumb;
        }

        if (_NotFoundThumb is null)
        {
            _NotFoundThumb = Util.DDS2Png(_CatFile, dir, "notfound");
        }

        return _NotFoundThumb;
    }
}

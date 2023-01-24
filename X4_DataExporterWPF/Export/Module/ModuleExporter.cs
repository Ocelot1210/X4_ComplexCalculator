using Dapper;
using LibX4.FileSystem;
using LibX4.Xml;
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
    /// サムネ画像管理クラス
    /// </summary>
    private readonly ThumbnailManager _ThumbnailManager;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="catFile">catファイルオブジェクト</param>
    /// <param name="waresXml">ウェア情報xml</param>
    public ModuleExporter(ICatFile catFile, XDocument waresXml)
    {
        ArgumentNullException.ThrowIfNull(waresXml.Root);

        _CatFile = catFile;
        _WaresXml = waresXml;
        _ThumbnailManager = new(catFile, "assets/fx/gui/textures/stationmodules", "notfound");
    }


    /// <inheritdoc/>
    public async Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            await connection.ExecuteAsync(@"
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
            var items = GetRecordsAsync(progress, cancellationToken);

            await connection.ExecuteAsync("INSERT INTO Module (ModuleID, ModuleTypeID, Macro, MaxWorkers, WorkersCapacity, NoBlueprint, Thumbnail) VALUES (@ModuleID, @ModuleTypeID, @Macro, @MaxWorkers, @WorkersCapacity, @NoBlueprint, @Thumbnail)", items);
        }
    }


    /// <summary>
    /// XML から Module データを読み出す
    /// </summary>
    /// <returns>読み出した Module データ</returns>
    internal async IAsyncEnumerable<Module> GetRecordsAsync(IProgress<(int currentStep, int maxSteps)>? progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var maxSteps = (int)(double)_WaresXml.Root!.XPathEvaluate("count(ware[contains(@tags, 'module')])");
        var currentStep = 0;

        foreach (var module in _WaresXml.Root!.XPathSelectElements("ware[contains(@tags, 'module')]"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((currentStep++, maxSteps));


            var moduleID = module.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(moduleID)) continue;

            var macroName = module.XPathSelectElement("component")?.Attribute("ref")?.Value;
            if (string.IsNullOrEmpty(macroName)) continue;

            var macroXml = await _CatFile.OpenIndexXmlAsync("index/macros.xml", macroName);
            if (macroXml?.Root is null) continue;

            var moduleTypeID = macroXml.Root.XPathSelectElement("macro")?.Attribute("class")?.Value;
            if (string.IsNullOrEmpty(moduleTypeID)) continue;

            // 従業員数/最大収容人数取得
            var workForce = macroXml?.Root?.XPathSelectElement("macro/properties/workforce");
            var maxWorkers = workForce?.Attribute("max")?.GetInt() ?? 0;
            var capacity = workForce?.Attribute("capacity")?.GetInt() ?? 0;

            var noBluePrint = module.Attribute("tags")?.Value.Contains("noblueprint") ?? false;

            yield return new Module(moduleID, moduleTypeID, macroName, maxWorkers, capacity, noBluePrint, await _ThumbnailManager.GetThumbnailAsync(macroName, cancellationToken));
        }

        progress?.Report((currentStep++, maxSteps));
    }
}

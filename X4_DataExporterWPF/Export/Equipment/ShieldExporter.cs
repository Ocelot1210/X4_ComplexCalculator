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
/// シールド情報抽出用クラス
/// </summary>
class ShieldExporter : IExporter
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
    /// コンストラクタ
    /// </summary>
    /// <param name="catFile">catファイルオブジェクト</param>
    /// <param name="waresXml">ウェア情報xml</param>
    public ShieldExporter(IIndexResolver catFile, XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS Shield
(
    EquipmentID     TEXT    NOT NULL PRIMARY KEY,
    Capacity        INTEGER NOT NULL,
    RechargeRate    INTEGER NOT NULL,
    RechargeDelay   REAL    NOT NULL,
    FOREIGN KEY (EquipmentID)   REFERENCES Equipment(EquipmentID)
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecordsAsync(progress, cancellationToken);

            await connection.ExecuteAsync("INSERT INTO Shield (EquipmentID, Capacity, RechargeRate, RechargeDelay) VALUES (@EquipmentID, @Capacity, @RechargeRate, @RechargeDelay)", items);
        }
    }


    /// <summary>
    /// XML から Shield データを読み出す
    /// </summary>
    /// <returns>読み出した Shield データ</returns>
    private async IAsyncEnumerable<Shield> GetRecordsAsync(IProgress<(int currentStep, int maxSteps)> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var maxSteps = (int)(double)_WaresXml.Root.XPathEvaluate("count(ware[@transport='equipment'][@group='shields'])");
        var currentStep = 0;


        foreach (var equipment in _WaresXml.Root.XPathSelectElements("ware[@transport='equipment'][@group='shields']"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report((currentStep++, maxSteps));


            var equipmentID = equipment.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(equipmentID)) continue;

            var macroName = equipment.XPathSelectElement("component")?.Attribute("ref")?.Value;
            if (string.IsNullOrEmpty(macroName)) continue;

            var macroXml = await _CatFile.OpenIndexXmlAsync("index/macros.xml", macroName, cancellationToken);
            XDocument componentXml;
            try
            {
                componentXml = await _CatFile.OpenIndexXmlAsync("index/components.xml", macroXml.Root.XPathSelectElement("macro/component").Attribute("ref").Value, cancellationToken);
            }
            catch
            {
                continue;
            }

            var idTag = macroXml.Root.XPathSelectElement("macro/properties/identification");
            if (idTag is null) continue;

            var rechargeElm = macroXml.Root.XPathSelectElement("macro/properties/recharge");
            if (rechargeElm is null) continue;

            yield return new Shield(
                equipmentID,
                rechargeElm.Attribute("max")?.GetInt() ?? 0,
                rechargeElm.Attribute("rate")?.GetInt() ?? 0,
                rechargeElm.Attribute("delay")?.GetDouble() ?? 0.0);
        }

        progress.Report((currentStep++, maxSteps));
    }
}

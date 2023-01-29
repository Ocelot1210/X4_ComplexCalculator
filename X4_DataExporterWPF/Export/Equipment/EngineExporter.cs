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
/// エンジン情報抽出用クラス
/// </summary>
class EngineExporter : IExporter
{
    /// <summary>
    /// catファイルオブジェクト
    /// </summary>
    private readonly IIndexResolver _catFile;


    /// <summary>
    /// ウェア情報xml
    /// </summary>
    private readonly XDocument _waresXml;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="catFile">catファイルオブジェクト</param>
    /// <param name="waresXml">ウェア情報xml</param>
    public EngineExporter(IIndexResolver catFile, XDocument waresXml)
    {
        ArgumentNullException.ThrowIfNull(waresXml.Root);

        _catFile = catFile;
        _waresXml = waresXml;
    }


    /// <inheritdoc/>
    public async Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            await connection.ExecuteAsync(@"
CREATE TABLE IF NOT EXISTS Engine
(
    EquipmentID         TEXT    NOT NULL PRIMARY KEY,
    ForwardThrust       REAL    NOT NULL,
    ReverseThrust       REAL    NOT NULL,
    BoostThrust         REAL    NOT NULL,
    BoostDuration       REAL    NOT NULL,
    BoostReleaseTime    REAL    NOT NULL,
    TravelThrust        REAL    NOT NULL,
    TravelReleaseTime   REAL    NOT NULL,
    FOREIGN KEY (EquipmentID)   REFERENCES Equipment(EquipmentID)
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecordsAsync(progress, cancellationToken);

            await connection.ExecuteAsync(@"
INSERT INTO Engine ( EquipmentID,  ForwardThrust,  ReverseThrust,  BoostThrust,  BoostDuration,  BoostReleaseTime,  TravelThrust,  TravelReleaseTime,  TravelThrust,  TravelReleaseTime)
            VALUES (@EquipmentID, @ForwardThrust, @ReverseThrust, @BoostThrust, @BoostDuration, @BoostReleaseTime, @TravelThrust, @TravelReleaseTime, @TravelThrust, @TravelReleaseTime)", items);
        }
    }


    /// <summary>
    /// XML から Engine データを読み出す
    /// </summary>
    /// <returns>読み出した Engine データ</returns>
    private async IAsyncEnumerable<Engine> GetRecordsAsync(IProgress<(int currentStep, int maxSteps)> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var maxSteps = (int)(double)_waresXml.Root!.XPathEvaluate("count(ware[@transport='equipment'][@group='engines'])");
        var currentStep = 0;


        foreach (var equipment in _waresXml.Root!.XPathSelectElements("ware[@transport='equipment'][@group='engines']"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report((currentStep++, maxSteps));


            var equipmentID = equipment.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(equipmentID)) continue;

            var macroName = equipment.XPathSelectElement("component")?.Attribute("ref")?.Value;
            if (string.IsNullOrEmpty(macroName)) continue;

            var macroXml = await _catFile.OpenIndexXmlAsync("index/macros.xml", macroName, cancellationToken);
            if (macroXml?.Root is null) continue;


            var thrust = macroXml.Root.XPathSelectElement("macro/properties/thrust");
            var boost = macroXml.Root.XPathSelectElement("macro/properties/boost");
            var travel = macroXml.Root.XPathSelectElement("macro/properties/travel");
            if (thrust is null || boost is null || travel is null) continue;

            var forwardThrust = thrust.Attribute("forward")?.GetDouble() ?? 0;

            yield return new Engine(
                equipmentID,
                forwardThrust,
                thrust.Attribute("reverse")?.GetInt() ?? 0,
                (int)(forwardThrust * boost.Attribute("thrust")?.GetDouble() ?? 1.0),
                boost.Attribute("duration")?.GetDouble() ?? 0.0,
                boost.Attribute("release")?.GetDouble() ?? 0.0,
                (int)(forwardThrust * travel.Attribute("thrust")?.GetDouble() ?? 1.0),
                travel.Attribute("release")?.GetDouble() ?? 0.0);
        }

        progress.Report((currentStep++, maxSteps));
    }
}

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
/// スラスター情報抽出用クラス
/// </summary>
class ThrusterExporter : IExporter
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
    public ThrusterExporter(IIndexResolver catFile, XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS Thruster
(
    EquipmentID     TEXT    NOT NULL PRIMARY KEY,
    ThrustStrafe    REAL    NOT NULL,
    ThrustPitch     REAL    NOT NULL,
    ThrustYaw       REAL    NOT NULL,
    ThrustRoll      REAL    NOT NULL,
    AngularRoll     REAL    NOT NULL,
    AngularPitch    REAL    NOT NULL,
    FOREIGN KEY (EquipmentID)   REFERENCES Equipment(EquipmentID)
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecords(progress, cancellationToken);

            await connection.ExecuteAsync(@"
INSERT INTO Thruster ( EquipmentID,  ThrustStrafe,  ThrustPitch,  ThrustYaw,  ThrustRoll,  AngularRoll,  AngularPitch)
            VALUES   (@EquipmentID, @ThrustStrafe, @ThrustPitch, @ThrustYaw, @ThrustRoll, @AngularRoll, @AngularPitch)", items);
        }
    }


    /// <summary>
    /// XML から Thruster データを読み出す
    /// </summary>
    /// <returns>読み出した Thruster データ</returns>
    private async IAsyncEnumerable<Thruster> GetRecords(IProgress<(int currentStep, int maxSteps)> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var maxSteps = (int)(double)_waresXml.Root!.XPathEvaluate("count(ware[@transport='equipment'][@group='thrusters'])");
        var currentStep = 0;


        foreach (var equipment in _waresXml.Root!.XPathSelectElements("ware[@transport='equipment'][@group='thrusters']"))
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
            var angular = macroXml.Root.XPathSelectElement("macro/properties/angular");
            if (thrust is null || angular is null) continue;


            yield return new Thruster(
                equipmentID,
                thrust.Attribute("strafe")?.GetDouble() ?? 0.0,
                thrust.Attribute("pitch")?.GetDouble()  ?? 0.0,
                thrust.Attribute("yaw")?.GetDouble()    ?? 0.0,
                thrust.Attribute("roll")?.GetDouble()   ?? 0.0,
                angular.Attribute("roll")?.GetDouble()  ?? 0.0,
                angular.Attribute("pitch")?.GetDouble() ?? 0.0
            );
        }

        progress.Report((currentStep++, maxSteps));
    }
}

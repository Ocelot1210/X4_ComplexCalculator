using Dapper;
using LibX4.FileSystem;
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

public class ShipPurposeExporter : IExporter
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
    public ShipPurposeExporter(IIndexResolver catFile, XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS ShipPurpose
(
    ShipID          TEXT    NOT NULL,
    Type            TEXT    NOT NULL,
    PurposeID       TEXT    NOT NULL,
    PRIMARY KEY (ShipID, Type),
    FOREIGN KEY (ShipID)    REFERENCES Ship(ShipID),
    FOREIGN KEY (PurposeID) REFERENCES Purpose(PurposeID)
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecordsAsync(progress, cancellationToken);

            await connection.ExecuteAsync(@"INSERT INTO ShipPurpose ( ShipID,  Type,  PurposeID) VALUES (@ShipID, @Type, @PurposeID)", items);
        }
    }



    /// <summary>
    /// レコード抽出
    /// </summary>
    private async IAsyncEnumerable<ShipPurpose> GetRecordsAsync(IProgress<(int currentStep, int maxSteps)> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var maxSteps = (int)(double)_waresXml.Root!.XPathEvaluate("count(ware[contains(@tags, 'ship')])");
        var currentStep = 0;


        foreach (var ship in _waresXml.Root!.XPathSelectElements("ware[contains(@tags, 'ship')]"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report((currentStep++, maxSteps));

            var shipID = ship.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(shipID)) continue;

            var macroName = ship.XPathSelectElement("component")?.Attribute("ref")?.Value;
            if (string.IsNullOrEmpty(macroName)) continue;

            var macroXml = await _catFile.OpenIndexXmlAsync("index/macros.xml", macroName, cancellationToken);
            if (macroXml?.Root is null) continue;

            var purpose = macroXml.Root.XPathSelectElement("macro/properties/purpose");
            if (purpose is null) continue;

            foreach (var attr in purpose.Attributes())
            {
                yield return new ShipPurpose(shipID, attr.Name.LocalName, attr.Value);
            }
        }

        progress.Report((currentStep++, maxSteps));
    }
}

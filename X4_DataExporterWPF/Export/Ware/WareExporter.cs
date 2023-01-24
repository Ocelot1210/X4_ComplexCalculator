using Dapper;
using LibX4.Lang;
using LibX4.Xml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;
using X4_DataExporterWPF.Internal;

namespace X4_DataExporterWPF.Export;

public class WareExporter : IExporter
{
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
    /// <param name="waresXml">ウェア情報xml</param>
    /// <param name="resolver">言語解決用オブジェクト</param>
    public WareExporter(XDocument waresXml, ILanguageResolver resolver)
    {
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
CREATE TABLE IF NOT EXISTS Ware
(
    WareID          TEXT    NOT NULL PRIMARY KEY,
    WareGroupID     TEXT,
    TransportTypeID TEXT,
    Name            TEXT    NOT NULL,
    Description     TEXT    NOT NULL,
    Volume          INTEGER NOT NULL,
    MinPrice        INTEGER NOT NULL,
    AvgPrice        INTEGER NOT NULL,
    MaxPrice        INTEGER NOT NULL,
    FOREIGN KEY (WareGroupID)       REFERENCES WareGroup(WareGroupID),
    FOREIGN KEY (TransportTypeID)   REFERENCES TransportType(TransportTypeID)
) WITHOUT ROWID");
        }

        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecords(progress, cancellationToken);

            await connection.ExecuteAsync("INSERT INTO Ware (WareID, WareGroupID, TransportTypeID, Name, Description, Volume, MinPrice, AvgPrice, MaxPrice) VALUES (@WareID, @WareGroupID, @TransportTypeID, @Name, @Description, @Volume, @MinPrice, @AvgPrice, @MaxPrice)", items);
        }
    }


    /// <summary>
    /// XML から Ware データを読み出す
    /// </summary>
    /// <returns>読み出した Ware データ</returns>
    private IEnumerable<Ware> GetRecords(IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        var maxSteps = (int)(double)_WaresXml.Root.XPathEvaluate("count(ware)");
        var currentStep = 0;


        foreach (var ware in _WaresXml.Root.XPathSelectElements("ware"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report((currentStep++, maxSteps));

            var wareID = ware.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(wareID)) continue;

            var wareGroupID = ware.Attribute("group")?.Value;

            var transportTypeID = ware.Attribute("transport")?.Value;

            var name = _Resolver.Resolve(ware.Attribute("name")?.Value ?? "");

            var description = _Resolver.Resolve(ware.Attribute("description")?.Value ?? "");
            var volume = ware.Attribute("volume")?.GetInt() ?? 1;

            var price = ware.Element("price");
            var minPrice = price?.Attribute("min")?.GetInt() ?? 0;
            var avgPrice = price?.Attribute("average")?.GetInt() ?? 0;
            var maxPrice = price?.Attribute("max")?.GetInt() ?? 0;

            yield return new Ware(wareID, wareGroupID, transportTypeID, name, description, volume, minPrice, avgPrice, maxPrice);
        }

        progress.Report((currentStep++, maxSteps));
    }
}

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

/// <summary>
/// 艦船のカーゴ情報抽出用クラス
/// </summary>
public class ShipTransportTypeExporter : IExporter
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
    /// <param name="resolver">言語解決用オブジェクト</param>
    public ShipTransportTypeExporter(IIndexResolver catFile, XDocument waresXml)
    {
        ArgumentNullException.ThrowIfNull(waresXml.Root);

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
CREATE TABLE IF NOT EXISTS ShipTransportType
(
    ShipID          TEXT    NOT NULL,
    TransportTypeID TEXT    NOT NULL,
    FOREIGN KEY (ShipID)            REFERENCES Ship(ShipID),
    FOREIGN KEY (TransportTypeID)   REFERENCES TransportType(TransportTypeID)
)");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecordsAsync(progress, cancellationToken);

            await connection.ExecuteAsync(@"INSERT INTO ShipTransportType(ShipID, TransportTypeID) VALUES(@ShipID, @TransportTypeID)", items);
        }
    }


    /// <summary>
    /// レコード抽出
    /// </summary>
    private async IAsyncEnumerable<ShipTransportType> GetRecordsAsync(IProgress<(int currentStep, int maxSteps)> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var maxSteps = (int)(double)_WaresXml.Root!.XPathEvaluate("count(ware[contains(@tags, 'ship')])");
        var currentStep = 0;

        foreach (var ship in _WaresXml.Root!.XPathSelectElements("ware[contains(@tags, 'ship')]"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report((currentStep++, maxSteps));

            var shipID = ship.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(shipID)) continue;

            var macroName = ship.XPathSelectElement("component")?.Attribute("ref")?.Value;
            if (string.IsNullOrEmpty(macroName)) continue;

            var macroXml = await _CatFile.OpenIndexXmlAsync("index/macros.xml", macroName, cancellationToken);

            foreach (var type in await GetCargoTypesAsync(macroXml, cancellationToken))
            {
                yield return new ShipTransportType(shipID, type);
            }
        }

        progress.Report((currentStep++, maxSteps));
    }


    /// <summary>
    /// カーゴ種別を取得する
    /// </summary>
    /// <param name="macroXml">マクロxml</param>
    /// <returns>該当艦船のカーゴ種別</returns>
    private async Task<IReadOnlyList<string>> GetCargoTypesAsync(XDocument macroXml, CancellationToken cancellationToken)
    {
        var componentName = macroXml.Root?.XPathSelectElement("macro/component")?.Attribute("ref")?.Value ?? "";
        if (string.IsNullOrEmpty(componentName)) return Array.Empty<string>();

        var componentXml = await _CatFile.OpenIndexXmlAsync("index/components.xml", componentName, cancellationToken);
        if (componentXml?.Root is null) return Array.Empty<string>();

        var connName = componentXml.Root.XPathSelectElement("component/connections/connection[contains(@tags, 'storage')]")?.Attribute("name")?.Value ?? "";
        if (string.IsNullOrEmpty(connName)) return Array.Empty<string>();

        var storage = macroXml.Root?.XPathSelectElement($"macro/connections/connection[@ref='{connName}']/macro")?.Attribute("ref")?.Value ?? "";
        if (string.IsNullOrEmpty(storage))
        {
            // カーゴが無い船(ゼノンの艦船等)を考慮
            return Array.Empty<string>();
        }

        var storageXml = await _CatFile.OpenIndexXmlAsync("index/macros.xml", storage, cancellationToken);
        if (storageXml?.Root is null) return Array.Empty<string>();

        var tags = storageXml.Root.XPathSelectElement("macro/properties/cargo")?.Attribute("tags")?.Value ?? "";

        return Util.SplitTags(tags);
    }
}

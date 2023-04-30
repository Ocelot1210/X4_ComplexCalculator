using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using LibX4.Xml;
using X4_DataExporterWPF.Entity;
using X4_DataExporterWPF.Internal;

namespace X4_DataExporterWPF.Export;

/// <summary>
/// ウェア生産時の追加効果情報抽出用クラス
/// </summary>
public class WareEffectExporter : IExporter
{
    /// <summary>
    /// ウェア情報xml
    /// </summary>
    private readonly XDocument _waresXml;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="waresXml">ウェア情報xml</param>
    public WareEffectExporter(XDocument waresXml)
    {
        ArgumentNullException.ThrowIfNull(waresXml.Root);
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
CREATE TABLE IF NOT EXISTS WareEffect
(
    WareID      TEXT    NOT NULL,
    Method      TEXT    NOT NULL,
    EffectID    TEXT    NOT NULL,
    Product     REAL    NOT NULL,
    PRIMARY KEY (WareID, Method, EffectID),
    FOREIGN KEY (WareID)    REFERENCES Ware(WareID),
    FOREIGN KEY (EffectID)  REFERENCES Effect(EffectID)
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecords(progress, cancellationToken);

            await connection.ExecuteAsync("INSERT INTO WareEffect (WareID, Method, EffectID, Product) VALUES (@WareID, @Method, @EffectID, @Product)", items);
        }
    }


    /// <summary>
    /// XML から WareEffect データを読み出す
    /// </summary>
    /// <returns>読み出した WareEffect データ</returns>
    internal IEnumerable<WareEffect> GetRecords(IProgress<(int currentStep, int maxSteps)>? progress, CancellationToken cancellationToken)
    {
        var maxSteps = (int)(double)_waresXml.Root!.XPathEvaluate("count(ware[contains(@tags, 'economy')])");
        var currentStep = 0;


        foreach (var ware in _waresXml.Root!.XPathSelectElements("ware[contains(@tags, 'economy')]"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((currentStep++, maxSteps));

            var wareID = ware.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(wareID)) continue;

            foreach (var prod in ware.XPathSelectElements("production"))
            {
                var method = prod.Attribute("method")?.Value;
                if (string.IsNullOrEmpty(method)) continue;

                foreach (var effect in prod.XPathSelectElements("effects/effect"))
                {
                    var effectID = effect.Attribute("type")?.Value;
                    if (string.IsNullOrEmpty(effectID)) continue;

                    var product = effect.Attribute("product")?.GetDouble() ?? 0.0;

                    yield return new WareEffect(wareID, method, effectID, product);
                }
            }
        }

        progress?.Report((currentStep++, maxSteps));
    }
}

using Dapper;
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

/// <summary>
/// ウェア生産に必要な情報抽出用クラス
/// </summary>
public class WareResourceExporter : IExporter
{
    /// <summary>
    /// 情報xml
    /// </summary>
    private readonly XDocument _waresXml;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="waresXml">ウェア情報xml</param>
    public WareResourceExporter(XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS WareResource
(
    WareID      TEXT    NOT NULL,
    Method      TEXT    NOT NULL,
    NeedWareID  TEXT    NOT NULL,
    Amount      INTEGER NOT NULL,
    PRIMARY KEY (WareID, Method, NeedWareID),
    FOREIGN KEY (WareID)        REFERENCES Ware(WareID),
    FOREIGN KEY (NeedWareID)    REFERENCES Ware(WareID)
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecords(progress, cancellationToken);

            await connection.ExecuteAsync("INSERT INTO WareResource (WareID, Method, NeedWareID, Amount) VALUES (@WareID, @Method, @NeedWareID, @Amount)", items);
        }
    }


    /// <summary>
    /// XML から WareResource データを読み出す
    /// </summary>
    /// <returns>読み出した WareResource データ</returns>
    private IEnumerable<WareResource> GetRecords(IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        var maxSteps = (int)(double)_waresXml.Root!.XPathEvaluate("count(ware)");
        var currentStep = 0;


        foreach (var ware in _waresXml.Root!.XPathSelectElements("ware"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report((currentStep++, maxSteps));


            var wareID = ware.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(wareID)) continue;

            var methods = new HashSet<string>();    // 生産方式(method)が重複しないように記憶するHashSet

            foreach (var prod in ware.XPathSelectElements("production"))
            {
                var method = prod.Attribute("method")?.Value;
                if (string.IsNullOrEmpty(method) || methods.Contains(method)) continue;
                methods.Add(method);

                foreach (var needWare in prod.XPathSelectElements("primary/ware"))
                {
                    var needWareID = needWare.Attribute("ware")?.Value;
                    if (string.IsNullOrEmpty(needWareID)) continue;

                    var amount = needWare.Attribute("amount")?.GetInt();
                    if (amount is null) continue;

                    yield return new WareResource(wareID, method, needWareID, amount.Value);
                }
            }
        }

        progress.Report((currentStep++, maxSteps));
    }
}

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

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// ウェア生産時の情報抽出用クラス
    /// </summary>
    public class WareProductionExporter : IExporter
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
        public WareProductionExporter(XDocument waresXml, ILanguageResolver resolver)
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
CREATE TABLE IF NOT EXISTS WareProduction
(
    WareID  TEXT    NOT NULL,
    Method  TEXT    NOT NULL,
    Name    TEXT    NOT NULL,
    Amount  INTEGER NOT NULL,
    Time    REAL    NOT NULL,
    PRIMARY KEY (WareID, Method),
    FOREIGN KEY (WareID)   REFERENCES Ware(WareID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords(progress, cancellationToken);

                await connection.ExecuteAsync("INSERT INTO WareProduction (WareID, Method, Name, Amount, Time) VALUES (@WareID, @Method, @Name, @Amount, @Time)", items);
            }
        }


        /// <summary>
        /// XML から WareProduction データを読み出す
        /// </summary>
        /// <returns>読み出した WareProduction データ</returns>
        private IEnumerable<WareProduction> GetRecords(IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
        {
            var maxSteps = (int)(double)_WaresXml.Root.XPathEvaluate("count(ware)");
            var currentStep = 0;


            foreach (var ware in _WaresXml.Root.XPathSelectElements("ware"))
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

                    var name = _Resolver.Resolve(prod.Attribute("name")?.Value ?? "");
                    var amount = prod.Attribute("amount").GetInt();
                    var time = prod.Attribute("time").GetDouble();

                    yield return new WareProduction(wareID, method, name, amount, time);
                }
            }

            progress.Report((currentStep++, maxSteps));
        }
    }
}

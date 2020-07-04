using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.Lang;
using X4_DataExporterWPF.Entity;

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
        private readonly LangageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waresXml">ウェア情報xml</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public WareProductionExporter(XDocument waresXml, LangageResolver resolver)
        {
            _WaresXml = waresXml;
            _Resolver = resolver;
        }


        /// <summary>
        /// 抽出処理
        /// </summary>
        /// <param name="cmd"></param>
        public void Export(IDbConnection connection)
        {
            //////////////////
            // テーブル作成 //
            //////////////////
            {
                connection.Execute(@"
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
                var items = _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'economy')]").SelectMany
                (
                    ware => ware.XPathSelectElements("production").Select
                    (
                        prod =>
                        {
                            var wareID = ware.Attribute("id")?.Value;
                            if (string.IsNullOrEmpty(wareID)) return null;

                            var method = prod.Attribute("method")?.Value;
                            if (string.IsNullOrEmpty(method)) return null;

                            var name = _Resolver.Resolve(prod.Attribute("name")?.Value ?? "");
                            var amount = int.Parse(prod.Attribute("amount")?.Value ?? "0");
                            var time = double.Parse(prod.Attribute("time")?.Value ?? "0.0");

                            return new WareProduction(wareID, method, name, amount, time);
                        }
                    )
                )
                .Where
                (
                    x => x != null
                );

                connection.Execute("INSERT INTO WareProduction (WareID, Method, Name, Amount, Time) VALUES (@WareID, @Method, @Name, @Amount, @Time)", items);
            }
        }
    }
}

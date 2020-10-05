using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.Lang;
using LibX4.Xml;
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


        /// <summary>
        /// 抽出処理
        /// </summary>
        /// <param name="connection"></param>
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
                var items = GetRecords();

                connection.Execute("INSERT INTO WareProduction (WareID, Method, Name, Amount, Time) VALUES (@WareID, @Method, @Name, @Amount, @Time)", items);
            }
        }


        /// <summary>
        /// XML から WareProduction データを読み出す
        /// </summary>
        /// <returns>読み出した WareProduction データ</returns>
        private IEnumerable<WareProduction> GetRecords()
        {
            foreach (var ware in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'economy')]"))
            {
                var wareID = ware.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(wareID)) continue;

                foreach (var prod in ware.XPathSelectElements("production"))
                {
                    var method = prod.Attribute("method")?.Value;
                    if (string.IsNullOrEmpty(method)) continue;

                    var name = _Resolver.Resolve(prod.Attribute("name")?.Value ?? "");
                    var amount = prod.Attribute("amount").GetInt();
                    var time = prod.Attribute("time").GetDouble();

                    yield return new WareProduction(wareID, method, name, amount, time);
                }
            }
        }
    }
}

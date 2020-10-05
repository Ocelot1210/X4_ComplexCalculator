using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.Xml;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// ウェア生産に必要な情報抽出用クラス
    /// </summary>
    public class WareResourceExporter : IExporter
    {
        /// <summary>
        /// 情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waresXml">ウェア情報xml</param>
        public WareResourceExporter(XDocument waresXml)
        {
            _WaresXml = waresXml;
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
                var items = GetRecords();

                connection.Execute("INSERT INTO WareResource (WareID, Method, NeedWareID, Amount) VALUES (@WareID, @Method, @NeedWareID, @Amount)", items);
            }
        }


        /// <summary>
        /// XML から WareResource データを読み出す
        /// </summary>
        /// <returns>読み出した WareResource データ</returns>
        private IEnumerable<WareResource> GetRecords()
        {
            foreach (var ware in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'economy')]"))
            {
                var wareID = ware.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(wareID)) continue;

                foreach (var prod in ware.XPathSelectElements("production"))
                {
                    var method = prod.Attribute("method")?.Value;
                    if (string.IsNullOrEmpty(method)) continue;

                    foreach (var needWare in prod.XPathSelectElements("primary/ware"))
                    {
                        var needWareID = needWare.Attribute("ware")?.Value;
                        if (string.IsNullOrEmpty(needWareID)) continue;

                        var amount = needWare.Attribute("amount").GetInt();
                        yield return new WareResource(wareID, method, needWareID, amount);
                    }
                }
            }
        }
    }
}

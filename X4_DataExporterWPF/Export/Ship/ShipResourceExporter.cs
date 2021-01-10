using Dapper;
using LibX4.Xml;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;


namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 艦船の建造リソースを抽出する
    /// </summary>
    class ShipResourceExporter : IExporter
    {
        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waresXml">ウェア情報xml</param>
        public ShipResourceExporter(XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS ShipResource
(
    ShipID      TEXT    NOT NULL,
    Method      TEXT    NOT NULL,
    WareID      TEXT    NOT NULL,
    Amount      INTEGER NOT NULL,
    PRIMARY KEY (ShipID,    Method, WareID),
    FOREIGN KEY (ShipID)    REFERENCES Ship(ShipID),
    FOREIGN KEY (WareID)    REFERENCES Ware(WareID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute("INSERT INTO ShipResource (ShipID, Method, WareID, Amount) VALUES (@ShipID, @Method, @WareID, @Amount)", items);
            }
        }


        /// <summary>
        /// XML から ModuleResource データを読み出す
        /// </summary>
        /// <returns>読み出した ModuleResource データ</returns>
        private IEnumerable<ShipResource> GetRecords()
        {
            foreach (var ship in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'ship')]"))
            {
                var shipID = ship.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(shipID)) continue;

                foreach (var prod in ship.XPathSelectElements("production"))
                {
                    var method = prod.Attribute("method")?.Value;
                    if (string.IsNullOrEmpty(method)) continue;

                    foreach (var ware in prod.XPathSelectElements("primary/ware"))
                    {
                        var wareID = ware.Attribute("ware")?.Value;
                        if (string.IsNullOrEmpty(wareID)) continue;

                        var amount = ware.Attribute("amount").GetInt();
                        yield return new ShipResource(shipID, method, wareID, amount);
                    }
                }
            }
        }
    }
}

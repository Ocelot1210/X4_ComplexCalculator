using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 従業員が必要とするウェア情報抽出用クラス
    /// </summary>
    class WorkUnitResourceExporter : IExporter
    {
        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waresXml">ウェア情報xml</param>
        public WorkUnitResourceExporter(XDocument waresXml)
        {
            _WaresXml = waresXml;
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
CREATE TABLE IF NOT EXISTS WorkUnitResource
(
    WorkUnitID  TEXT    NOT NULL,
    Method      TEXT    NOT NULL,
    WareID      TEXT    NOT NULL,
    Amount      INTEGER NOT NULL,
    PRIMARY KEY (WorkUnitID, Method, WareID),
    FOREIGN KEY (WareID)   REFERENCES Ware(WareID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute("INSERT INTO WorkUnitResource (WorkUnitID, Method, WareID, Amount) VALUES (@WorkUnitID, @Method, @WareID, @Amount)", items);
            }
        }


        /// <summary>
        /// XML から WorkUnitResource データを読み出す
        /// </summary>
        /// <returns>読み出した WorkUnitResource データ</returns>
        private IEnumerable<WorkUnitResource> GetRecords()
        {
            foreach (var workUnit in _WaresXml.Root.XPathSelectElements("ware[@transport='workunit']"))
            {
                var workUnitID = workUnit.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(workUnitID)) continue;

                foreach (var prod in workUnit.XPathSelectElements("production"))
                {
                    var method = prod.Attribute("method")?.Value;
                    if (string.IsNullOrEmpty(method)) continue;

                    foreach (var ware in prod.XPathSelectElements("primary/ware"))
                    {
                        var wareID = ware.Attribute("ware")?.Value;
                        if (string.IsNullOrEmpty(wareID)) continue;

                        var amount = int.Parse(ware.Attribute("amount")?.Value ?? "0");

                        yield return new WorkUnitResource(workUnitID, method, wareID, amount);
                    }
                }
            }
        }
    }
}

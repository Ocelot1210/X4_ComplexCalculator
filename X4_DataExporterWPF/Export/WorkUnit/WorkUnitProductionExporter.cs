using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 従業員用生産情報抽出用クラス
    /// </summary>
    class WorkUnitProductionExporter : IExporter
    {
        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waresXml">ウェア情報xml</param>
        public WorkUnitProductionExporter(XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS WorkUnitProduction
(
    WorkUnitID  TEXT    NOT NULL,
    Time        INTEGER NOT NULL,
    Amount      INTEGER NOT NULL,
    Method      TEXT    NOT NULL,
    PRIMARY KEY (WorkUnitID, Method)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = _WaresXml.Root.XPathSelectElements("ware[@transport='workunit']").SelectMany
                (
                    workUnit => workUnit.XPathSelectElements("production").Select
                    (
                        prod =>
                        {
                            var workUnitID = workUnit.Attribute("id")?.Value;
                            if (string.IsNullOrEmpty(workUnitID)) return null;

                            var time = int.Parse(prod.Attribute("time")?.Value ?? "0");
                            var amount = int.Parse(prod.Attribute("amount")?.Value ?? "0");

                            var method = prod.Attribute("method")?.Value;
                            if (string.IsNullOrEmpty(method)) return null;

                            return new WorkUnitProduction(workUnitID, time, amount, method);
                        }
                    )
                )
                .Where
                (
                    x => x != null
                );

                connection.Execute("INSERT INTO WorkUnitProduction (WorkUnitID, Time, Amount, Method) VALUES (@WorkUnitID, @Time, @Amount, @Method)", items);
            }
        }
    }
}

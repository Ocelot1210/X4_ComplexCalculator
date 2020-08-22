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
    /// モジュール建造に必要なウェア情報抽出用クラス
    /// </summary>
    public class ModuleResourceExporter : IExporter
    {
        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waresXml">ウェア情報xml</param>
        public ModuleResourceExporter(XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS ModuleResource
(
    ModuleID    TEXT    NOT NULL,
    Method      TEXT    NOT NULL,
    WareID      TEXT    NOT NULL,
    Amount      INTEGER NOT NULL,
    PRIMARY KEY (ModuleID, Method, WareID),
    FOREIGN KEY (ModuleID)  REFERENCES Module(ModuleID),
    FOREIGN KEY (WareID)    REFERENCES Ware(WareID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute("INSERT INTO ModuleResource (ModuleID, Method, WareID, Amount) VALUES (@ModuleID, @Method, @WareID, @Amount)", items);
            }
        }


        /// <summary>
        /// XML から ModuleResource データを読み出す
        /// </summary>
        /// <returns>読み出した ModuleResource データ</returns>
        private IEnumerable<ModuleResource> GetRecords()
        {
            foreach (var module in _WaresXml.Root.XPathSelectElements("ware[@tags='module']"))
            {
                var moduleID = module.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(moduleID)) continue;

                foreach (var prod in module.XPathSelectElements("production"))
                {
                    var method = prod.Attribute("method")?.Value;
                    if (string.IsNullOrEmpty(method)) continue;

                    foreach (var ware in prod.XPathSelectElements("primary/ware"))
                    {
                        var wareID = ware.Attribute("ware")?.Value;
                        if (string.IsNullOrEmpty(wareID)) continue;

                        var amount = ware.Attribute("amount").GetInt();
                        yield return new ModuleResource(moduleID, method, wareID, amount);
                    }
                }
            }
        }
    }
}

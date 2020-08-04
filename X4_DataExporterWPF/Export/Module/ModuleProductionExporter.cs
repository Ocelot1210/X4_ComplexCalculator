using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// モジュール建造に関する情報抽出用クラス
    /// </summary>
    class ModuleProductionExporter : IExporter
    {
        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waresXml">ウェア情報xml</param>
        public ModuleProductionExporter(XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS ModuleProduction
(
    ModuleID    TEXT    NOT NULL,
    Method      TEXT    NOT NULL,
    Time        REAL    NOT NULL,
    PRIMARY KEY (ModuleID, Method),
    FOREIGN KEY (ModuleID)  REFERENCES Module(ModuleID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();


                connection.Execute("INSERT INTO ModuleProduction (ModuleID, Method, Time) VALUES (@ModuleID, @Method, @Time)", items);
            }
        }


        /// <summary>
        /// XML から ModuleProduction データを読み出す
        /// </summary>
        /// <returns>読み出した ModuleProduction データ</returns>
        private IEnumerable<ModuleProduction> GetRecords()
        {
            foreach (var module in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'module')]"))
            {
                var moduleID = module.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(moduleID)) continue;

                foreach (var prod in module.XPathSelectElements("production"))
                {
                    var method = prod.Attribute("method")?.Value;
                    if (string.IsNullOrEmpty(method)) continue;

                    double time = double.Parse(prod.Attribute("time")?.Value ?? "0.0");
                    yield return new ModuleProduction(moduleID, method, time);
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.FileSystem;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// モジュールの生産品情報抽出用クラス
    /// </summary>
    public class ModuleProductExporter : IExporter
    {
        /// <summary>
        /// catファイルオブジェクト
        /// </summary>
        private readonly IIndexResolver _CatFile;

        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="waresXml">ウェア情報xml</param>
        public ModuleProductExporter(IIndexResolver catFile, XDocument waresXml)
        {
            _CatFile = catFile;
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
CREATE TABLE IF NOT EXISTS ModuleProduct
(
    ModuleID    TEXT    NOT NULL,
    WareID      TEXT    NOT NULL,
    Method      TEXT    NOT NULL,
    PRIMARY KEY (ModuleID, WareID, Method),
    FOREIGN KEY (ModuleID)  REFERENCES Module(ModuleID),
    FOREIGN KEY (WareID)    REFERENCES Ware(WareID)
)WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute("INSERT INTO ModuleProduct (ModuleID, WareID, Method) VALUES (@ModuleID, @WareID, @Method)", items);
            }
        }


        /// <summary>
        /// XML から ModuleProduct データを読み出す
        /// </summary>
        /// <returns>読み出した ModuleProduct データ</returns>
        private IEnumerable<ModuleProduct> GetRecords()
        {
            foreach (var module in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'module')]"))
            {
                var moduleID = module.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(moduleID)) continue;

                var macroName = module.XPathSelectElement("component").Attribute("ref").Value;
                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);

                foreach (var queue in macroXml.Root.XPathSelectElements("macro/properties/production/queue"))
                {
                    var wareID = queue.Attribute("ware")?.Value ?? "";
                    var method = queue.Attribute("method")?.Value ?? "";
                    if (string.IsNullOrEmpty(wareID) || string.IsNullOrEmpty(method)) continue;

                    yield return new ModuleProduct(moduleID, wareID, method);
                }
            }
        }
    }
}

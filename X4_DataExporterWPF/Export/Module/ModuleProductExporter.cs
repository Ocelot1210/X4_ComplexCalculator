using System.Data;
using System.Linq;
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
        private readonly CatFile _CatFile;

        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="waresXml">ウェア情報xml</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public ModuleProductExporter(CatFile catFile, XDocument waresXml)
        {
            _CatFile = catFile;
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
                var items = _WaresXml.Root.XPathSelectElements("ware[@tags='module']").Select
                (
                    module => GetRecord(module)
                )
                .Where
                (
                    x => x != null
                );

                connection.Execute("INSERT INTO ModuleProduct (ModuleID, WareID, Method) VALUES (@ModuleID, @WareID, @Method)", items);
            }
        }


        /// <summary>
        /// 1レコード分の情報抽出
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        private ModuleProduct? GetRecord(XElement module)
        {
            try
            {
                var moduleID = module.Attribute("id").Value;
                if (string.IsNullOrEmpty(moduleID)) return null;

                var macroName = module.XPathSelectElement("component").Attribute("ref").Value;
                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);
                var prod = macroXml.Root.XPathSelectElement("macro/properties/production/queue");

                var wareID = prod?.Attribute("ware")?.Value;
                if (string.IsNullOrEmpty(wareID)) return null;

                var method = prod?.Attribute("method")?.Value ?? "default";

                return new ModuleProduct(moduleID, wareID, method);
            }
            catch
            {
                return null;
            }
        }
    }
}

using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.FileSystem;
using LibX4.Lang;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// モジュール情報抽出用クラス
    /// </summary>
    public class ModuleExporter : IExporter
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
        /// 言語解決用オブジェクト
        /// </summary>
        private readonly LangageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="waresXml">ウェア情報xml</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public ModuleExporter(CatFile catFile, XDocument waresXml, LangageResolver resolver)
        {
            _CatFile = catFile;
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
CREATE TABLE IF NOT EXISTS Module
(
    ModuleID        TEXT    NOT NULL PRIMARY KEY,
    ModuleTypeID    TEXT    NOT NULL,
    Name            TEXT    NOT NULL,
    Macro           TEXT    NOT NULL,
    MaxWorkers      INTEGER NOT NULL,
    WorkersCapacity INTEGER NOT NULL,
    FOREIGN KEY (ModuleTypeID)  REFERENCES ModuleType(ModuleTypeID)
) WITHOUT ROWID");
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

                connection.Execute("INSERT INTO Module (ModuleID, ModuleTypeID, Name, Macro, MaxWorkers, WorkersCapacity) VALUES (@ModuleID, @ModuleTypeID, @Name, @Macro, @MaxWorkers, @WorkersCapacity)", items);
            }
        }


        /// <summary>
        /// 1レコード分の情報抽出
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        private Module? GetRecord(XElement module)
        {
            try
            {
                var moduleID = module.Attribute("id").Value;
                if (string.IsNullOrEmpty(moduleID)) return null;

                var macroName = module.XPathSelectElement("component").Attribute("ref").Value;
                if (string.IsNullOrEmpty(macroName)) return null;

                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);
                var moduleTypeID = macroXml.Root.XPathSelectElement("macro").Attribute("class").Value;
                if (string.IsNullOrEmpty(moduleTypeID)) return null;

                // 従業員数/最大収容人数取得
                var workForce = macroXml?.Root?.XPathSelectElement("macro/properties/workforce");
                var maxWorkers = int.Parse(workForce?.Attribute("max")?.Value ?? "0");
                var capacity = int.Parse(workForce?.Attribute("capacity")?.Value ?? "0");

                var name = _Resolver.Resolve(module.Attribute("name")?.Value ?? "");
                name = string.IsNullOrEmpty(name) ? macroName : name;

                return new Module(moduleID, moduleTypeID, name, macroName, maxWorkers, capacity);
            }
            catch
            {
                return null;
            }
        }
    }
}

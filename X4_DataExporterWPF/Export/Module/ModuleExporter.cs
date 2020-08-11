using System.Collections.Generic;
using System.Data;
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
        private readonly IIndexResolver _CatFile;

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
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="waresXml">ウェア情報xml</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public ModuleExporter(IIndexResolver catFile, XDocument waresXml, ILanguageResolver resolver)
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
    NoBlueprint     INTEGER NOT NULL,
    FOREIGN KEY (ModuleTypeID)  REFERENCES ModuleType(ModuleTypeID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute("INSERT INTO Module (ModuleID, ModuleTypeID, Name, Macro, MaxWorkers, WorkersCapacity, NoBlueprint) VALUES (@ModuleID, @ModuleTypeID, @Name, @Macro, @MaxWorkers, @WorkersCapacity, @NoBlueprint)", items);
            }
        }


        /// <summary>
        /// XML から Module データを読み出す
        /// </summary>
        /// <returns>読み出した Module データ</returns>
        internal IEnumerable<Module> GetRecords()
        {
            foreach (var module in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'module')]"))
            {
                var moduleID = module.Attribute("id").Value;
                if (string.IsNullOrEmpty(moduleID)) continue;

                var macroName = module.XPathSelectElement("component").Attribute("ref").Value;
                if (string.IsNullOrEmpty(macroName)) continue;

                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);
                var moduleTypeID = macroXml.Root.XPathSelectElement("macro").Attribute("class").Value;
                if (string.IsNullOrEmpty(moduleTypeID)) continue;

                // 従業員数/最大収容人数取得
                var workForce = macroXml?.Root?.XPathSelectElement("macro/properties/workforce");
                var maxWorkers = int.Parse(workForce?.Attribute("max")?.Value ?? "0");
                var capacity = int.Parse(workForce?.Attribute("capacity")?.Value ?? "0");

                var name = _Resolver.Resolve(module.Attribute("name")?.Value ?? "");
                name = string.IsNullOrEmpty(name) ? macroName : name;

                var noBluePrint = module.Attribute("tags").Value.Contains("noblueprint") ? 1 : 0;

                yield return new Module(moduleID, moduleTypeID, name, macroName, maxWorkers, capacity, noBluePrint);
            }
        }
    }
}

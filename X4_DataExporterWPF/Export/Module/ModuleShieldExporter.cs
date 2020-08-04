using System.Collections.Generic;
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
    /// モジュールのシールド情報抽出用クラス
    /// </summary>
    public class ModuleShieldExporter : IExporter
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
        public ModuleShieldExporter(CatFile catFile, XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS ModuleShield
(
    ModuleID    TEXT    NOT NULL,
    SizeID      TEXT    NOT NULL,
    Amount      INTEGER NOT NULL,
    PRIMARY KEY (ModuleID, SizeID),
    FOREIGN KEY (ModuleID)  REFERENCES Module(ModuleID),
    FOREIGN KEY (SizeID)    REFERENCES Size(SizeID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();


                connection.Execute("INSERT INTO ModuleShield (ModuleID, SizeID, Amount) VALUES (@ModuleID, @SizeID, @Amount)", items);
            }
        }


        /// <summary>
        /// XML から ModuleShield データを読み出す
        /// </summary>
        /// <returns>読み出した ModuleShield データ</returns>
        private IEnumerable<ModuleShield> GetRecords()
        {
            foreach (var module in _WaresXml.Root.XPathSelectElements("ware[@tags='module']"))
            {
                var moduleID = module.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(moduleID)) continue;

                var macroName = module.XPathSelectElement("component").Attribute("ref").Value;
                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);
                var componentXml = _CatFile.OpenIndexXml("index/components.xml", macroXml.Root.XPathSelectElement("macro/component").Attribute("ref").Value);

                // 装備集計用辞書
                var sizeDict = new Dictionary<string, int>()
                {
                    { "extrasmall", 0 },
                    { "small",      0 },
                    { "medium",     0 },
                    { "large",      0 },
                    { "extralarge", 0 }
                };


                // シールドが記載されているタグを取得する
                foreach (var connections in componentXml.Root.XPathSelectElements("component/connections/connection[contains(@tags, 'shield')]"))
                {
                    // シールドのサイズを取得
                    var attr = connections.Attribute("tags").Value;
                    var size = sizeDict.Keys.Where(x => attr.Contains(x)).FirstOrDefault();

                    if (string.IsNullOrEmpty(size)) continue;

                    sizeDict[size]++;
                }

                foreach (var (sizeID, amount) in sizeDict.Where(x => 0 < x.Value))
                {
                    yield return new ModuleShield(moduleID, sizeID, amount);
                }
            }
        }
    }
}

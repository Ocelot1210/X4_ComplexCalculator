using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// モジュール所有派閥情報抽出用クラス
    /// </summary>
    public class ModuleOwnerExporter : IExporter
    {
        /// <summary>
        /// 情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waresXml">ウェア情報xml</param>
        public ModuleOwnerExporter(XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS ModuleOwner
(
    ModuleID    TEXT    NOT NULL,
    FactionID   TEXT    NOT NULL,
    PRIMARY KEY (ModuleID, FactionID),
    FOREIGN KEY (ModuleID)  REFERENCES Module(ModuleID),
    FOREIGN KEY (FactionID) REFERENCES Faction(FactionID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();


                connection.Execute("INSERT INTO ModuleOwner (ModuleID, FactionID) VALUES (@ModuleID, @FactionID)", items);
            }
        }


        /// <summary>
        /// XML から ModuleOwner データを読み出す
        /// </summary>
        /// <returns>読み出した ModuleOwner データ</returns>
        internal IEnumerable<ModuleOwner> GetRecords()
        {
            foreach (var module in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'module')]"))
            {
                var moduleID = module.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(moduleID)) continue;

                var owners = module.XPathSelectElements("owner")
                    .Select(owner => owner.Attribute("faction")?.Value)
                    .Where(factionID => !string.IsNullOrEmpty(factionID))
                    .Distinct();
                foreach (var factionID in owners)
                {
                    if (factionID == null) continue;
                    yield return new ModuleOwner(moduleID, factionID);
                }
            }
        }
    }
}

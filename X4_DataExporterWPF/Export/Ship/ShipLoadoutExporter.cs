using Dapper;
using LibX4.FileSystem;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;
using LibX4.Xml;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 艦船のロードアウト情報を抽出する
    /// </summary>
    public class ShipLoadoutExporter : IExporter
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
        public ShipLoadoutExporter(IIndexResolver catFile, XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS ShipLoadout
(
    ShipID      TEXT    NOT NULL,
    LoadoutID   TEXT    NOT NULL,
    MacroName   TEXT    NOT NULL,
    Count       INTEGER NOT NULL
)");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute(@"INSERT INTO ShipLoadout(ShipID, LoadoutID, MacroName, Count) VALUES (@ShipID, @LoadoutID, @MacroName, @Count)", items);
            }
        }


        /// <summary>
        /// レコード抽出
        /// </summary>
        private IEnumerable<ShipLoadout> GetRecords()
        {
            foreach (var ship in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'ship')]"))
            {
                var shipID = ship.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(shipID)) continue;

                var macroName = ship.XPathSelectElement("component").Attribute("ref").Value;
                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);
                if (macroXml is null) continue;

                foreach (var loadout in macroXml.Root.XPathSelectElements("macro/properties/loadouts/loadout"))
                {
                    var loadoutID = loadout.Attribute("id")?.Value;
                    if (string.IsNullOrEmpty(loadoutID)) continue;

                    // 抽出要素一覧
                    string[] xpathes = { "macros/engine", "macros/shield", "groups/shields", "groups/turrets" };

                    foreach (var xpath in xpathes)
                    {
                        var equipments = loadout.XPathSelectElements(xpath)
                            .Select(x => (Macro: x.Attribute("macro")?.Value ?? "", Exact: x.Attribute("exact")?.GetInt() ?? 1))
                            .Where(x => !string.IsNullOrEmpty(x.Macro))
                            .GroupBy(x => x.Macro);
                        foreach (var equipment in equipments)
                        {
                            yield return new ShipLoadout(shipID, loadoutID, equipment.Key, equipment.Sum(x => x.Exact));
                        }
                    }
                }
            }

            yield break;
        }
    }
}

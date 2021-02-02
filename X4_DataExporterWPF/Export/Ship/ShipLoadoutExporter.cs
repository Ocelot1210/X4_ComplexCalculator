using Dapper;
using LibX4.FileSystem;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;
using LibX4.Xml;
using System;

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
        public void Export(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress)
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
    GroupName   TEXT    NOT NULL,
    Count       INTEGER NOT NULL
)");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords(progress);

                connection.Execute(@"INSERT INTO ShipLoadout(ShipID, LoadoutID, MacroName, GroupName, Count) VALUES (@ShipID, @LoadoutID, @MacroName, @GroupName, @Count)", items);
            }
        }


        /// <summary>
        /// レコード抽出
        /// </summary>
        private IEnumerable<ShipLoadout> GetRecords(IProgress<(int currentStep, int maxSteps)> progress)
        {
            var maxSteps = (int)(double)_WaresXml.Root.XPathEvaluate("count(ware[contains(@tags, 'ship')])");
            var currentStep = 0;


            foreach (var ship in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'ship')]"))
            {
                progress.Report((currentStep++, maxSteps));

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
                            .Select(x => (Macro: x.Attribute("macro")?.Value ?? "", GroupName: x.Attribute("group")?.Value ?? "", Exact: x.Attribute("exact")?.GetInt() ?? 1))
                            .Where(x => !string.IsNullOrEmpty(x.Macro) && !string.IsNullOrEmpty(x.GroupName));
                        foreach (var (macro, groupName, exact) in equipments)
                        {
                            yield return new ShipLoadout(shipID, loadoutID, macro, groupName, exact);
                        }
                    }
                }
            }

            progress.Report((currentStep++, maxSteps));
        }
    }
}

using Dapper;
using LibX4.FileSystem;
using LibX4.Xml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;
using X4_DataExporterWPF.Internal;

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


        /// <inheritdoc/>
        public async Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
        {
            //////////////////
            // テーブル作成 //
            //////////////////
            {
                await connection.ExecuteAsync(@"
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
                var items = GetRecordsAsync(progress, cancellationToken);

                await connection.ExecuteAsync(@"INSERT INTO ShipLoadout(ShipID, LoadoutID, MacroName, GroupName, Count) VALUES (@ShipID, @LoadoutID, @MacroName, @GroupName, @Count)", items);
            }
        }


        /// <summary>
        /// レコード抽出
        /// </summary>
        private async IAsyncEnumerable<ShipLoadout> GetRecordsAsync(IProgress<(int currentStep, int maxSteps)> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var maxSteps = (int)(double)_WaresXml.Root.XPathEvaluate("count(ware[contains(@tags, 'ship')])");
            var currentStep = 0;


            foreach (var ship in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'ship')]"))
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress.Report((currentStep++, maxSteps));

                var shipID = ship.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(shipID)) continue;

                var macroName = ship.XPathSelectElement("component")?.Attribute("ref")?.Value;
                if (string.IsNullOrEmpty(macroName)) continue;

                var macroXml = await _CatFile.OpenIndexXmlAsync("index/macros.xml", macroName, cancellationToken);
                if (macroXml is null) continue;

                foreach (var loadout in macroXml.Root.XPathSelectElements("macro/properties/loadouts/loadout"))
                {
                    var loadoutID = loadout.Attribute("id")?.Value;
                    if (string.IsNullOrEmpty(loadoutID)) continue;

                    // 抽出要素一覧
                    string[] xpathes = { "macros", "groups", "virtualmacros" };

                    foreach (var xpath in xpathes)
                    {
                        var equipments = loadout.XPathSelectElements(xpath)
                            .SelectMany(x => x.Elements())
                            .Select(x => (Macro: x.Attribute("macro")?.Value ?? "", GroupName: x.Attribute("group")?.Value ?? "", Exact: x.Attribute("exact")?.GetInt() ?? 1))
                            .Where(x => !string.IsNullOrEmpty(x.Macro));
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

using Dapper;
using LibX4.FileSystem;
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
    /// ウェアの装備情報を抽出する
    /// </summary>
    public class WareEquipmentExporter : IExporter
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
        /// 装備のタグ情報
        /// </summary>
        private readonly LinkedList<IReadOnlyList<WareEquipmentTag>> _EquipmentTags = new();


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="waresXml">ウェア情報xml</param>
        public WareEquipmentExporter(IIndexResolver catFile, XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS WareEquipment
(
    WareID          TEXT    NOT NULL,
    GroupName       TEXT    NOT NULL,
    ConnectionName  TEXT    NOT NULL,
    EquipmentTypeID TEXT    NOT NULL,
    PRIMARY KEY (WareID, ConnectionName, GroupName),
    FOREIGN KEY (WareID)            REFERENCES Ware(WareID),
    FOREIGN KEY (EquipmentTypeID)   REFERENCES EquipmentType(EquipmentTypeID)
) WITHOUT ROWID");

                await connection.ExecuteAsync(@"
CREATE TABLE IF NOT EXISTS WareEquipmentTag
(
    WareID          TEXT    NOT NULL,
    ConnectionName  TEXT    NOT NULL,
    Tag             TEXT    NOT NULL,
    FOREIGN KEY (WareID)            REFERENCES Ware(WareID),
    FOREIGN KEY (ConnectionName)    REFERENCES WareEquipment(ConnectionName)
)");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetUniqueRecordsAsync(progress, cancellationToken);

                await connection.ExecuteAsync(@"INSERT INTO WareEquipment (WareID, GroupName, ConnectionName, EquipmentTypeID) VALUES (@WareID, @GroupName, @ConnectionName, @EquipmentTypeID)", items);

                await connection.ExecuteAsync(@"INSERT INTO WareEquipmentTag (WareID, ConnectionName, Tag) VALUES (@WareID, @ConnectionName, @Tag)", _EquipmentTags.SelectMany(x => x));
            }
        }


        /// <summary>
        /// ユニークなレコード抽出
        /// </summary>
        private async IAsyncEnumerable<WareEquipment> GetUniqueRecordsAsync(IProgress<(int currentStep, int maxSteps)> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var hash = new HashSet<(string, string)>();
            await foreach (var item in GetRecordsAsync(progress, cancellationToken))
            {
                if (hash.Contains((item.WareID, item.ConnectionName)))
                {
                    continue;
                }
                hash.Add((item.WareID, item.ConnectionName));
                yield return item;
            }
        }


        /// <summary>
        /// レコード抽出
        /// </summary>
        private async IAsyncEnumerable<WareEquipment> GetRecordsAsync(IProgress<(int currentStep, int maxSteps)> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var maxSteps = (int)(double)_WaresXml.Root.XPathEvaluate("count(ware)");
            var currentStep = 0;


            foreach (var ware in _WaresXml.Root.XPathSelectElements("ware"))
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress.Report((currentStep++, maxSteps));


                var wareID = ware.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(wareID)) continue;

                var macroName = ware.XPathSelectElement("component")?.Attribute("ref")?.Value;
                if (string.IsNullOrEmpty(macroName)) continue;

                var macroXml = await _CatFile.OpenIndexXmlAsync("index/macros.xml", macroName, cancellationToken);
                if (macroXml is null) continue;

                var componentXml = await _CatFile.OpenIndexXmlAsync("index/components.xml", macroXml.Root.XPathSelectElement("macro/component").Attribute("ref").Value, cancellationToken);
                if (componentXml is null) continue;

                var equipmentTags = new List<WareEquipmentTag>();

                // 抽出対象装備種別一覧 (装備種別ID, tags内文字列)
                (string equipmentTypeID, string tagValue)[] pairs = { ("weapons", "weapon"), ("turrets", "turret"), ("shields", "shield"), ("engines", "engine") };
                foreach (var (equipmentTypeID, tagValue) in pairs)
                {
                    var connections = componentXml.Root.XPathSelectElements($"component/connections/connection[contains(@tags, '{tagValue}')]");
                    foreach (var connection in connections)
                    {
                        var name = connection.Attribute("name")?.Value;
                        if (string.IsNullOrEmpty(name)) continue;

                        equipmentTags.AddRange(Util.SplitTags(connection.Attribute("tags")?.Value).Select(x => new WareEquipmentTag(wareID, name, x)));
                        yield return new WareEquipment(wareID, name.Trim(), equipmentTypeID, connection.Attribute("group")?.Value.Trim() ?? "");
                    }
                }


                // スラスターを抽出
                var thruster = macroXml.Root.XPathSelectElement("macro/properties/thruster")?.Attribute("tags")?.Value;
                if (thruster is not null)
                {
                    const string thrusterConnectionName = "thruster";

                    equipmentTags.AddRange(Util.SplitTags(thruster).Select(x => new WareEquipmentTag(wareID, thrusterConnectionName, x)));
                    yield return new WareEquipment(wareID, thrusterConnectionName, "thrusters", "");
                }

                if (equipmentTags.Any())
                {
                    _EquipmentTags.AddLast(equipmentTags);
                }
            }

            progress.Report((currentStep++, maxSteps));
        }
    }
}

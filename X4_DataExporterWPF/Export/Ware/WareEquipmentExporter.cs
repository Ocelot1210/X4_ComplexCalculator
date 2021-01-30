using Dapper;
using LibX4.FileSystem;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;

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

                connection.Execute(@"
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
                var items = GetRecords2();

                connection.Execute(@"INSERT INTO WareEquipment (WareID, GroupName, ConnectionName, EquipmentTypeID) VALUES (@WareID, @GroupName, @ConnectionName, @EquipmentTypeID)", items);

                connection.Execute(@"INSERT INTO WareEquipmentTag (WareID, ConnectionName, Tag) VALUES (@WareID, @ConnectionName, @Tag)", _EquipmentTags.SelectMany(x => x));
            }
        }

        private IEnumerable<WareEquipment> GetRecords2()
        {
            var hash = new HashSet<(string, string)>();
            foreach (var item in GetRecords())
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
        private IEnumerable<WareEquipment> GetRecords()
        {
            foreach (var ware in _WaresXml.Root.XPathSelectElements("ware"))
            {
                var wareID = ware.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(wareID)) continue;

                var macroName = ware.XPathSelectElement("component")?.Attribute("ref")?.Value;
                if (string.IsNullOrEmpty(macroName)) continue;

                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);
                if (macroXml is null) continue;

                var componentXml = _CatFile.OpenIndexXml("index/components.xml", macroXml.Root.XPathSelectElement("macro/component").Attribute("ref").Value);
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
                        yield return new WareEquipment(wareID, name, equipmentTypeID, connection.Attribute("group")?.Value ?? "");
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
        }
    }
}

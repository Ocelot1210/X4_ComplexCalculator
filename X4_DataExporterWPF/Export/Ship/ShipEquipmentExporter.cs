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
    /// 艦船の装備情報を抽出する
    /// </summary>
    public class ShipEquipmentExporter : IExporter
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
        public ShipEquipmentExporter(IIndexResolver catFile, XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS ShipEquipment
(
    ShipID          TEXT    NOT NULL,
    EquipmentTypeID TEXT    NOT NULL,
    SizeID          TEXT    NOT NULL,
    Count           INTEGER NOT NULL,
    PRIMARY KEY (ShipID, EquipmentTypeID, SizeID),
    FOREIGN KEY (ShipID)            REFERENCES Ship(ShipID),
    FOREIGN KEY (EquipmentTypeID)   REFERENCES EquipmentType(EquipmentTypeID),
    FOREIGN KEY (SizeID)            REFERENCES Size(SizeID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute(@"INSERT INTO ShipEquipment (ShipID, EquipmentTypeID, SizeID, Count) VALUES (@ShipID, @EquipmentTypeID, @SizeID, @Count)", items);
            }
        }



        /// <summary>
        /// レコード抽出
        /// </summary>
        private IEnumerable<ShipEquipment> GetRecords()
        {
            foreach (var ship in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'ship')]"))
            {
                var shipID = ship.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(shipID)) continue;

                var macroName = ship.XPathSelectElement("component").Attribute("ref").Value;
                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);
                if (macroXml is null) continue;

                var componentXml = _CatFile.OpenIndexXml("index/components.xml", macroXml.Root.XPathSelectElement("macro/component").Attribute("ref").Value);
                if (componentXml is null) continue;

                // 抽出対象装備種別一覧 (装備種別ID, tags内文字列)
                (string, string)[] equipmentTypeIDs = { ("weapons", "weapon"), ("turrets", "turret"), ("shields", "shield"), ("engines", "engine") };
                foreach (var type in equipmentTypeIDs)
                {
                    foreach (var equipment in GetEquipment(componentXml, type.Item2))
                    {
                        yield return new ShipEquipment(shipID, type.Item1, equipment.Item1, equipment.Item2);
                    }
                }


                // スラスターを抽出
                var thruster = macroXml.Root.XPathSelectElement("macro/properties/thruster")?.Attribute("tags")?.Value;
                if (thruster is null) continue;
                foreach (var size in thruster.Split(" "))
                {
                    yield return new ShipEquipment(shipID, "thrusters", size, 1);
                }
            }
        }


        /// <summary>
        /// 装備を取得する
        /// </summary>
        /// <param name="componentXml">components xml</param>
        /// <param name="equipmentTypeID">装備種別ID</param>
        /// <returns>サイズIDと個数のタプル</returns>
        private IEnumerable<(string, int)> GetEquipment(XDocument componentXml, string equipmentTypeID)
        {
            // 装備集計用辞書
            var sizeDict = new Dictionary<string, int>()
            {
                { "extrasmall", 0 },
                { "small",      0 },
                { "medium",     0 },
                { "large",      0 },
                { "extralarge", 0 }
            };

            // 指定した装備が記載されているタグを取得する
            foreach (var connection in componentXml.Root.XPathSelectElements($"component/connections/connection[contains(@tags, '{equipmentTypeID}')]"))
            {
                // 装備のサイズを取得する
                var attr = connection.Attribute("tags").Value;
                var size = sizeDict.Keys.FirstOrDefault(x => attr.Contains(x));

                if (string.IsNullOrEmpty(size)) continue;

                sizeDict[size]++;
            }


            foreach (var (sizeID, amount) in sizeDict.Where(x => 0 < x.Value))
            {
                yield return (sizeID, amount);
            }
        }
    }
}

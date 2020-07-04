using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 装備保有派閥抽出用クラス
    /// </summary>
    class EquipmentOwnerExporter : IExporter
    {
        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="xml">ウェア情報xml</param>
        public EquipmentOwnerExporter(XDocument xml)
        {
            _WaresXml = xml;
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
CREATE TABLE IF NOT EXISTS EquipmentOwner
(
    EquipmentID TEXT    NOT NULL,
    FactionID   TEXT    NOT NULL,
    PRIMARY KEY (EquipmentID, FactionID),
    FOREIGN KEY (EquipmentID)   REFERENCES Equipment(EquipmentID),
    FOREIGN KEY (FactionID)     REFERENCES Faction(FactionID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = _WaresXml.Root.XPathSelectElements("ware[@transport='equipment']").SelectMany
                (
                    equipment =>
                    {
                        var equipmentID = equipment.Attribute("id")?.Value;
                        if (string.IsNullOrEmpty(equipmentID)) return null;

                        return equipment.XPathSelectElements("owner")
                            .Select(owner => owner.Attribute("faction")?.Value)
                            .Where(factionID => !string.IsNullOrEmpty(factionID))
                            .Distinct()
                            .Select(factionID =>
                            {
                                if (factionID == null)
                                {
                                    return null;
                                }
                                else
                                {
                                    return new EquipmentOwner(equipmentID, factionID);
                                }
                            });
                    }
                )
                .Where
                (
                    x => x != null
                );

                connection.Execute("INSERT INTO EquipmentOwner (EquipmentID, FactionID) VALUES (@EquipmentID, @FactionID)", items);
            }
        }
    }
}

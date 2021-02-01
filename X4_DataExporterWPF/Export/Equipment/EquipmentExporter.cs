using Dapper;
using LibX4.FileSystem;
using LibX4.Lang;
using LibX4.Xml;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;
using System.Linq;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 装備情報抽出用クラス
    /// </summary>
    class EquipmentExporter : IExporter
    {
        /// <summary>
        /// catファイルオブジェクト
        /// </summary>
        private readonly ICatFile _CatFile;


        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// サムネが見つからない場合のサムネ
        /// </summary>
        private byte[]? _NotFoundThumb;


        /// <summary>
        /// 装備のタグ一覧
        /// </summary>
        private readonly LinkedList<IReadOnlyList<EquipmentTag>> _EquipmentTags = new();


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="waresXml">ウェア情報xml</param>
        public EquipmentExporter(ICatFile catFile, XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS Equipment
(
    EquipmentID     TEXT    NOT NULL PRIMARY KEY,
    MacroName       TEXT    NOT NULL,
    EquipmentTypeID TEXT    NOT NULL,
    Hull            INTEGER NOT NULL,
    HullIntegrated  BOOLEAN NOT NULL,
    Mk              INTEGER NOT NULL,
    MakerRace       TEXT,
    Thumbnail       BLOB,
    FOREIGN KEY (EquipmentID)       REFERENCES Ware(WareID),
    FOREIGN KEY (EquipmentTypeID)   REFERENCES EquipmentType(EquipmentTypeID),
    FOREIGN KEY (MakerRace)         REFERENCES Race(RaceID)
) WITHOUT ROWID");

                connection.Execute(@"
CREATE TABLE IF NOT EXISTS EquipmentTag
(
    EquipmentID     TEXT    NOT NULL,
    Tag             TEXT    NOT NULL,
    FOREIGN KEY (EquipmentID)       REFERENCES Equipment(EquipmentID)
)");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute(@"
INSERT INTO Equipment ( EquipmentID,  MacroName,  EquipmentTypeID,  Hull,  HullIntegrated,  Mk,  MakerRace,  Thumbnail)
            VALUES    (@EquipmentID, @MacroName, @EquipmentTypeID, @Hull, @HullIntegrated, @Mk, @MakerRace, @Thumbnail)", items);

                connection.Execute("INSERT INTO EquipmentTag (EquipmentID, Tag) VALUES (@EquipmentID, @Tag)", _EquipmentTags.SelectMany(x => x));
            }
        }


        /// <summary>
        /// XML から Equipment データを読み出す
        /// </summary>
        /// <returns>読み出した Equipment データ</returns>
        private IEnumerable<Equipment> GetRecords()
        {
            foreach (var equipment in _WaresXml.Root.XPathSelectElements("ware[@transport='equipment']"))
            {
                var equipmentID = equipment.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(equipmentID)) continue;

                var macroName = equipment.XPathSelectElement("component")?.Attribute("ref")?.Value;
                if (string.IsNullOrEmpty(macroName)) continue;

                var equipmentTypeID = equipment.Attribute("group")?.Value;
                if (string.IsNullOrEmpty(equipmentTypeID)) continue;

                
                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);
                XDocument componentXml;
                try
                {
                    componentXml = _CatFile.OpenIndexXml("index/components.xml", macroXml.Root.XPathSelectElement("macro/component").Attribute("ref").Value);
                }
                catch
                {
                    continue;
                }

                // 装備が記載されているタグを取得する
                var component = componentXml.Root.XPathSelectElement("component/connections/connection[contains(@tags, 'component')]");

                // タグがあれば格納する
                var tags = Util.SplitTags(component?.Attribute("tags")?.Value).Distinct();
                if (tags.Any())
                {
                    _EquipmentTags.AddLast(tags.Select(x => new EquipmentTag(equipmentID, x)).ToArray());
                }

                var idElm = macroXml.Root.XPathSelectElement("macro/properties/identification");
                if (idElm is null) continue;

                yield return new Equipment(
                    equipmentID,
                    macroName,
                    equipmentTypeID,
                    macroXml.Root.XPathSelectElement("macro/properties/hull")?.Attribute("max")?.GetInt() ?? 0,
                    (macroXml.Root.XPathSelectElement("macro/properties/hull")?.Attribute("integrated")?.GetInt() ?? 0) == 1,
                    idElm.Attribute("mk")?.GetInt() ?? 0,
                    idElm.Attribute("makerrace")?.Value,
                    GetThumbnail(macroName)
                );
            }
        }


        /// <summary>
        /// サムネ画像を取得する
        /// </summary>
        /// <param name="macroName">マクロ名</param>
        /// <returns>サムネ画像のバイト配列</returns>
        private byte[]? GetThumbnail(string macroName)
        {
            const string dir = "assets/fx/gui/textures/upgrades";
            var thumb = Util.DDS2Png(_CatFile, dir, macroName);
            if (thumb is not null)
            {
                return thumb;
            }

            if (_NotFoundThumb is null)
            {
                _NotFoundThumb = Util.DDS2Png(_CatFile, dir, "notfound");
            }

            return _NotFoundThumb;
        }
    }
}

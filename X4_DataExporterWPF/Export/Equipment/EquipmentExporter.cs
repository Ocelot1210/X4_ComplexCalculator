using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.FileSystem;
using LibX4.Lang;
using LibX4.Xml;
using X4_DataExporterWPF.Entity;

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
        private readonly IIndexResolver _CatFile;


        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// 言語解決用オブジェクト
        /// </summary>
        private readonly ILanguageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="waresXml">ウェア情報xml</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public EquipmentExporter(IIndexResolver catFile, XDocument waresXml, ILanguageResolver resolver)
        {
            _CatFile = catFile;
            _WaresXml = waresXml;
            _Resolver = resolver;
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
    SizeID          TEXT    NOT NULL,
    Name            TEXT    NOT NULL,
    Hull            INTEGER NOT NULL,
    HullIntegrated  BOOLEAN NOT NULL,
    Mk              INTEGER NOT NULL,
    MakerRace       TEXT,
    Description     TEXT    NOT NULL,
    FOREIGN KEY (EquipmentTypeID)   REFERENCES EquipmentType(EquipmentTypeID),
    FOREIGN KEY (SizeID)            REFERENCES Size(SizeID),
    FOREIGN KEY (MakerRace)         REFERENCES Race(RaceID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute(@"
INSERT INTO Equipment ( EquipmentID,  MacroName,  EquipmentTypeID,  SizeID,  Name,  Hull,  HullIntegrated,  Mk,  MakerRace,  Description)
            VALUES    (@EquipmentID, @MacroName, @EquipmentTypeID, @SizeID, @Name, @Hull, @HullIntegrated, @Mk, @MakerRace, @Description)", items);
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

                // サイズ一覧
                string[] sizes = { "extrasmall", "small", "medium", "large", "extralarge" };

                // 一致するサイズを探す
                var tags = component?.Attribute("tags")?.Value.Split(" ");
                var sizeID = sizes.FirstOrDefault(x => tags?.Contains(x) == true);
                // 一致するサイズがなかった場合
                if (string.IsNullOrEmpty(sizeID)) continue;

                var name = _Resolver.Resolve(equipment.Attribute("name").Value);
                name = string.IsNullOrEmpty(name) ? macroName : name;

                var idElm = macroXml.Root.XPathSelectElement("macro/properties/identification");
                if (idElm is null) continue;

                yield return new Equipment(
                    equipmentID,
                    macroName,
                    equipmentTypeID,
                    sizeID,
                    name,
                    macroXml.Root.XPathSelectElement("macro/properties/hull")?.Attribute("max")?.GetInt() ?? 0,
                    (macroXml.Root.XPathSelectElement("macro/properties/hull")?.Attribute("integrated")?.GetInt() ?? 0) == 1,
                    idElm.Attribute("mk")?.GetInt() ?? 0,
                    idElm.Attribute("makerrace")?.Value,
                    _Resolver.Resolve(idElm.Attribute("description")?.Value ?? ""));
            }
        }
    }
}

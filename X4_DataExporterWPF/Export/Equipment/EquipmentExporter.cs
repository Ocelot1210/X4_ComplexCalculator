using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.FileSystem;
using LibX4.Lang;
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
        private readonly CatFile _CatFile;


        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// 言語解決用オブジェクト
        /// </summary>
        private readonly LangageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="waresXml">ウェア情報xml</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public EquipmentExporter(CatFile catFile, XDocument waresXml, LangageResolver resolver)
        {
            _CatFile = catFile;
            _WaresXml = waresXml;
            _Resolver = resolver;
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
CREATE TABLE IF NOT EXISTS Equipment
(
    EquipmentID     TEXT    NOT NULL PRIMARY KEY,
    MacroName       TEXT    NOT NULL,
    EquipmentTypeID TEXT    NOT NULL,
    SizeID          TEXT    NOT NULL,
    Name            TEXT    NOT NULL,
    FOREIGN KEY (EquipmentTypeID)   REFERENCES EquipmentType(EquipmentTypeID),
    FOREIGN KEY (SizeID)            REFERENCES Size(SizeID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = _WaresXml.Root.XPathSelectElements("ware[@transport='equipment']").Select
                (
                    equipment => GetRecord(equipment)
                )
                .Where
                (
                    x => x != null
                );


                connection.Execute("INSERT INTO Equipment (EquipmentID, MacroName, EquipmentTypeID, SizeID, Name) VALUES (@EquipmentID, @MacroName, @EquipmentTypeID, @SizeID, @Name)", items);
            }
        }


        /// <summary>
        /// 1レコード分の情報を抽出する
        /// </summary>
        /// <param name="equipment"></param>
        /// <returns></returns>
        private Equipment? GetRecord(XElement equipment)
        {
            try
            {
                var equipmentID = equipment.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(equipmentID)) return null;

                var macroName = equipment.XPathSelectElement("component").Attribute("ref")?.Value;
                if (string.IsNullOrEmpty(macroName)) return null;

                var equipmentTypeID = equipment.Attribute("group")?.Value;
                if (string.IsNullOrEmpty(equipmentTypeID)) return null;

                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);
                var componentXml = _CatFile.OpenIndexXml("index/components.xml", macroXml.Root.XPathSelectElement("macro/component").Attribute("ref").Value);

                // 装備が記載されているタグを取得する
                var component = componentXml.Root.XPathSelectElement("component/connections/connection[contains(@tags, 'component')]");

                // サイズ一覧
                string[] sizes = { "extrasmall", "small", "medium", "large", "extralarge" };

                // 一致するサイズを探す
                var tags = component?.Attribute("tags").Value.Split(" ");
                var sizeID = sizes.Where(x => tags?.Contains(x) == true).FirstOrDefault();
                // 一致するサイズがなかった場合
                if (string.IsNullOrEmpty(sizeID)) return null;

                var name = _Resolver.Resolve(equipment.Attribute("name").Value);
                name = string.IsNullOrEmpty(name) ? macroName : name;

                return new Equipment(equipmentID, macroName, equipmentTypeID, sizeID, name);
            }
            catch
            {
                return null;
            }
        }
    }
}

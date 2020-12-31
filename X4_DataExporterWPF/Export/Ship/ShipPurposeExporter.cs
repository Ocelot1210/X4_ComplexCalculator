using Dapper;
using LibX4.FileSystem;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;


namespace X4_DataExporterWPF.Export
{
    public class ShipPurposeExporter : IExporter
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
        public ShipPurposeExporter(IIndexResolver catFile, XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS ShipPurpose
(
    ShipID          TEXT    NOT NULL,
    Type            TEXT    NOT NULL,
    PurposeID       TEXT    NOT NULL,
    FOREIGN KEY (ShipID)    REFERENCES Ship(ShipID),
    FOREIGN KEY (PurposeID) REFERENCES Purpose(PurposeID)
)");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute(@"INSERT INTO ShipPurpose ( ShipID,  Type,  PurposeID) VALUES (@ShipID, @Type, @PurposeID)", items);
            }
        }



        /// <summary>
        /// レコード抽出
        /// </summary>
        private IEnumerable<ShipPurpose> GetRecords()
        {
            foreach (var ship in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'ship')]"))
            {
                var shipID = ship.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(shipID)) continue;

                var macroName = ship.XPathSelectElement("component").Attribute("ref").Value;
                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);

                var purpose = macroXml.Root.XPathSelectElement("macro/properties/purpose");
                if (purpose is null) continue;

                foreach (var attr in purpose.Attributes())
                {
                    yield return new ShipPurpose(shipID, attr.Name.LocalName, attr.Value);
                }
            }
        }
    }
}

using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// ウェアのタグ情報を抽出するクラス
    /// </summary>
    class WareTagsExporter : IExporter
    {
        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waresXml">ウェア情報xml</param>
        public WareTagsExporter(XDocument waresXml)
        {
            _WaresXml = waresXml;
        }


        public void Export(IDbConnection connection)
        {
            //////////////////
            // テーブル作成 //
            //////////////////
            {
                connection.Execute(@"
CREATE TABLE IF NOT EXISTS WareTags
(
    WareID  TEXT    NOT NULL,
    Tag     TEXT    NOT NULL,
    PRIMARY KEY (WareID, Tag),
    FOREIGN KEY (WareID)   REFERENCES Ware(WareID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute("INSERT INTO WareTags (WareID, Tag) VALUES (@WareID, @Tag)", items);
            }
        }



        private IEnumerable<WareTag> GetRecords()
        {
            foreach (var ware in _WaresXml.Root.XPathSelectElements("ware"))
            {
                var wareID = ware.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(wareID)) continue;

                var tags = Util.SplitTags(ware.Attribute("tags")?.Value).Distinct();

                foreach (var tag in tags)
                {
                    yield return new WareTag(wareID, tag);
                }
            }
        }
    }
}

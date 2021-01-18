using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.Lang;
using LibX4.Xml;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    public class WareExporter : IExporter
    {
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
        /// <param name="waresXml">ウェア情報xml</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public WareExporter(XDocument waresXml, ILanguageResolver resolver)
        {
            _WaresXml = waresXml;
            _Resolver = resolver;
        }


        /// <summary>
        /// データ抽出
        /// </summary>
        /// <param name="connection"></param>
        public void Export(IDbConnection connection)
        {
            //////////////////
            // テーブル作成 //
            //////////////////
            {
                connection.Execute(@"
CREATE TABLE IF NOT EXISTS Ware
(
    WareID          TEXT    NOT NULL PRIMARY KEY,
    WareGroupID     TEXT,
    TransportTypeID TEXT,
    Name            TEXT    NOT NULL,
    Description     TEXT    NOT NULL,
    Volume          INTEGER NOT NULL,
    MinPrice        INTEGER NOT NULL,
    AvgPrice        INTEGER NOT NULL,
    MaxPrice        INTEGER NOT NULL,
    FOREIGN KEY (WareGroupID)       REFERENCES WareGroup(WareGroupID),
    FOREIGN KEY (TransportTypeID)   REFERENCES TransportType(TransportTypeID)
) WITHOUT ROWID");
            }

            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute("INSERT INTO Ware (WareID, WareGroupID, TransportTypeID, Name, Description, Volume, MinPrice, AvgPrice, MaxPrice) VALUES (@WareID, @WareGroupID, @TransportTypeID, @Name, @Description, @Volume, @MinPrice, @AvgPrice, @MaxPrice)", items);
            }
        }


        /// <summary>
        /// XML から Ware データを読み出す
        /// </summary>
        /// <returns>読み出した Ware データ</returns>
        private IEnumerable<Ware> GetRecords()
        {
            foreach (var ware in _WaresXml.Root.XPathSelectElements("ware"))
            {
                var wareID = ware.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(wareID)) continue;

                var wareGroupID = ware.Attribute("group")?.Value;

                var transportTypeID = ware.Attribute("transport")?.Value;

                var name = _Resolver.Resolve(ware.Attribute("name")?.Value ?? "");

                var description = _Resolver.Resolve(ware.Attribute("description")?.Value ?? "");
                var volume = ware.Attribute("volume")?.GetInt() ?? 1;

                var price = ware.Element("price");
                var minPrice = price?.Attribute("min")?.GetInt()     ?? 0;
                var avgPrice = price?.Attribute("average")?.GetInt() ?? 0;
                var maxPrice = price?.Attribute("max")?.GetInt()     ?? 0;

                yield return new Ware(wareID, wareGroupID, transportTypeID, name, description, volume, minPrice, avgPrice, maxPrice);
            }
        }
    }
}

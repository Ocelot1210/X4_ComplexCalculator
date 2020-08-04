using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.Lang;
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
        private readonly LanguageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waresXml">ウェア情報xml</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public WareExporter(XDocument waresXml, LanguageResolver resolver)
        {
            _WaresXml = waresXml;
            _Resolver = resolver;
        }


        /// <summary>
        /// データ抽出
        /// </summary>
        /// <param name="cmd"></param>
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
    WareGroupID     TEXT    NOT NULL,
    TransportTypeID TEXT    NOT NULL,
    Name            TEXT    NOT NULL,
    Description     TEXT    NOT NULL,
    FactoryName     TEXT    NOT NULL,
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

                connection.Execute("INSERT INTO Ware (WareID, WareGroupID, TransportTypeID, Name, Description, FactoryName, Volume, MinPrice, AvgPrice, MaxPrice) VALUES (@WareID, @WareGroupID, @TransportTypeID, @Name, @Description, @FactoryName, @Volume, @MinPrice, @AvgPrice, @MaxPrice)", items);
            }
        }


        /// <summary>
        /// XML から Ware データを読み出す
        /// </summary>
        /// <returns>読み出した Ware データ</returns>
        private IEnumerable<Ware> GetRecords()
        {
            foreach (var ware in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'economy')]"))
            {
                var wareID = ware.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(wareID)) continue;

                var wareGroupID = ware.Attribute("group")?.Value;
                if (string.IsNullOrEmpty(wareGroupID)) continue;

                var transportTypeID = ware.Attribute("transport")?.Value;
                if (string.IsNullOrEmpty(transportTypeID)) continue;

                var name = _Resolver.Resolve(ware.Attribute("name")?.Value ?? "");
                if (string.IsNullOrEmpty(name)) continue;

                var description = _Resolver.Resolve(ware.Attribute("description")?.Value ?? "");
                var factoryName = _Resolver.Resolve(ware.Attribute("factoryname")?.Value ?? "");
                var volume = int.Parse(ware.Attribute("volume")?.Value ?? "0");

                var price = ware.Element("price");
                var minPrice = int.Parse(price.Attribute("min")?.Value ?? "0");
                var avgPrice = int.Parse(price.Attribute("average")?.Value ?? "0");
                var maxPrice = int.Parse(price.Attribute("max")?.Value ?? "0");

                yield return new Ware(wareID, wareGroupID, transportTypeID, name, description, factoryName, volume, minPrice, avgPrice, maxPrice);
            }
        }
    }
}

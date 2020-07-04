using System.Data;
using System.Linq;
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
        private readonly LangageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waresXml">ウェア情報xml</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public WareExporter(XDocument waresXml, LangageResolver resolver)
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
                var items = _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'economy')]").Select
                (x =>
                {
                    var wareID = x.Attribute("id")?.Value;
                    if (string.IsNullOrEmpty(wareID)) return null;

                    var wareGroupID = x.Attribute("group")?.Value;
                    if (string.IsNullOrEmpty(wareGroupID)) return null;

                    var transportTypeID = x.Attribute("transport")?.Value;
                    if (string.IsNullOrEmpty(transportTypeID)) return null;

                    var name = _Resolver.Resolve(x.Attribute("name")?.Value ?? "");
                    if (string.IsNullOrEmpty(name)) return null;

                    var description = _Resolver.Resolve(x.Attribute("description")?.Value ?? "");
                    var factoryName = _Resolver.Resolve(x.Attribute("factoryname")?.Value ?? "");
                    var volume = int.Parse(x.Attribute("volume")?.Value ?? "0");

                    var price = x.Element("price");
                    var minPrice = int.Parse(price.Attribute("min")?.Value ?? "0");
                    var avgPrice = int.Parse(price.Attribute("average")?.Value ?? "0");
                    var maxPrice = int.Parse(price.Attribute("max")?.Value ?? "0");

                    return new Ware(wareID, wareGroupID, transportTypeID, name, description, factoryName, volume, minPrice, avgPrice, maxPrice);
                })
                .Where
                (
                    x => x != null
                );

                connection.Execute("INSERT INTO Ware (WareID, WareGroupID, TransportTypeID, Name, Description, FactoryName, Volume, MinPrice, AvgPrice, MaxPrice) VALUES (@WareID, @WareGroupID, @TransportTypeID, @Name, @Description, @FactoryName, @Volume, @MinPrice, @AvgPrice, @MaxPrice)", items);
            }
        }
    }
}

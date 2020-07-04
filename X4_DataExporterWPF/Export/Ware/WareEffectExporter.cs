using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// ウェア生産時の追加効果情報抽出用クラス
    /// </summary>
    public class WareEffectExporter : IExporter
    {
        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waresXml">ウェア情報xml</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public WareEffectExporter(XDocument waresXml)
        {
            _WaresXml = waresXml;
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
CREATE TABLE IF NOT EXISTS WareEffect
(
    WareID      TEXT    NOT NULL,
    Method      TEXT    NOT NULL,
    EffectID    TEXT    NOT NULL,
    Product     REAL    NOT NULL,
    PRIMARY KEY (WareID, Method, EffectID),
    FOREIGN KEY (WareID)    REFERENCES Ware(WareID),
    FOREIGN KEY (EffectID)  REFERENCES Effect(EffectID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'economy')]").SelectMany
                (
                    ware => ware.XPathSelectElements("production").SelectMany
                    (
                        prod => prod.XPathSelectElements("effects/effect").Select
                        (
                            effect =>
                            {
                                var wareID = ware.Attribute("id")?.Value;
                                if (string.IsNullOrEmpty(wareID)) return null;

                                var method = prod.Attribute("method")?.Value;
                                if (string.IsNullOrEmpty(method)) return null;

                                var effectID = effect.Attribute("type")?.Value;
                                if (string.IsNullOrEmpty(effectID)) return null;

                                var product = double.Parse(effect.Attribute("product")?.Value ?? "0.0");

                                return new WareEffect(wareID, method, effectID, product);
                            }
                        )
                    )
                )
                .Where
                (
                    x => x != null
                );

                connection.Execute("INSERT INTO WareEffect (WareID, Method, EffectID, Product) VALUES (@WareID, @Method, @EffectID, @Product)", items);
            }
        }
    }
}

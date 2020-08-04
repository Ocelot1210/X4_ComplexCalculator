using System.Collections.Generic;
using System.Data;
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
                var items = GetRecords();

                connection.Execute("INSERT INTO WareEffect (WareID, Method, EffectID, Product) VALUES (@WareID, @Method, @EffectID, @Product)", items);
            }
        }


        /// <summary>
        /// XML から WareEffect データを読み出す
        /// </summary>
        /// <returns>読み出した WareEffect データ</returns>
        private IEnumerable<WareEffect> GetRecords()
        {
            foreach (var ware in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'economy')]"))
            {
                var wareID = ware.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(wareID)) continue;

                foreach (var prod in ware.XPathSelectElements("production"))
                {
                    var method = prod.Attribute("method")?.Value;
                    if (string.IsNullOrEmpty(method)) continue;

                    foreach (var effect in prod.XPathSelectElements("effects/effect"))
                    {
                        var effectID = effect.Attribute("type")?.Value;
                        if (string.IsNullOrEmpty(effectID)) continue;

                        double.TryParse(effect.Attribute("product")?.Value, out var product);

                        yield return new WareEffect(wareID, method, effectID, product);
                    }
                }
            }
        }
    }
}

using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
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
        public WareEffectExporter(XDocument waresXml)
        {
            _WaresXml = waresXml;
        }

        /// <summary>
        /// 抽出処理
        /// </summary>
        /// <param name="connection"></param>
        public void Export(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress)
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
                var items = GetRecords(progress);

                connection.Execute("INSERT INTO WareEffect (WareID, Method, EffectID, Product) VALUES (@WareID, @Method, @EffectID, @Product)", items);
            }
        }


        /// <summary>
        /// XML から WareEffect データを読み出す
        /// </summary>
        /// <returns>読み出した WareEffect データ</returns>
        internal IEnumerable<WareEffect> GetRecords(IProgress<(int currentStep, int maxSteps)>? progress = null)
        {
            var maxSteps = (int)(double)_WaresXml.Root.XPathEvaluate("count(ware[contains(@tags, 'economy')])");
            var currentStep = 0;


            foreach (var ware in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'economy')]"))
            {
                progress?.Report((currentStep++, maxSteps));

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

            progress?.Report((currentStep++, maxSteps));
        }
    }
}

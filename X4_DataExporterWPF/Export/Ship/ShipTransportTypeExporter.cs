using Dapper;
using LibX4.FileSystem;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 艦船のカーゴ情報抽出用クラス
    /// </summary>
    public class ShipTransportTypeExporter : IExporter
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
        /// <param name="resolver">言語解決用オブジェクト</param>
        public ShipTransportTypeExporter(IIndexResolver catFile, XDocument waresXml)
        {
            _CatFile = catFile;
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
CREATE TABLE IF NOT EXISTS ShipTransportType
(
    ShipID          TEXT    NOT NULL,
    TransportTypeID TEXT    NOT NULL,
    FOREIGN KEY (ShipID)            REFERENCES Ship(ShipID),
    FOREIGN KEY (TransportTypeID)   REFERENCES TransportType(TransportTypeID)
)");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords(progress);

                connection.Execute(@"INSERT INTO ShipTransportType(ShipID, TransportTypeID) VALUES(@ShipID, @TransportTypeID)", items);
            }
        }


        /// <summary>
        /// レコード抽出
        /// </summary>
        private IEnumerable<ShipTransportType> GetRecords(IProgress<(int currentStep, int maxSteps)> progress)
        {
            var maxSteps = (int)(double)_WaresXml.Root.XPathEvaluate("count(ware[contains(@tags, 'ship')])");
            var currentStep = 0;

            foreach (var ship in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'ship')]"))
            {
                progress.Report((currentStep++, maxSteps));

                var shipID = ship.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(shipID)) continue;

                var macroName = ship.XPathSelectElement("component").Attribute("ref").Value;
                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);

                foreach (var type in EnumerateCargoTypes(macroXml))
                {
                    yield return new ShipTransportType(shipID, type);
                }
            }

            progress.Report((currentStep++, maxSteps));
        }


        /// <summary>
        /// カーゴ種別を列挙する
        /// </summary>
        /// <param name="macroXml">マクロxml</param>
        /// <returns>該当艦船のカーゴ種別</returns>
        private IEnumerable<string> EnumerateCargoTypes(XDocument macroXml)
        {
            var componentName = macroXml.Root.XPathSelectElement("macro/component")?.Attribute("ref")?.Value ?? "";
            if (string.IsNullOrEmpty(componentName)) return Enumerable.Empty<string>();

            var componentXml = _CatFile.OpenIndexXml("index/components.xml", componentName);
            if (componentXml is null) return Enumerable.Empty<string>();

            var connName = componentXml.Root.XPathSelectElement("component/connections/connection[contains(@tags, 'storage')]")?.Attribute("name")?.Value ?? "";
            if (string.IsNullOrEmpty(connName)) return Enumerable.Empty<string>();

            var storage = macroXml.Root.XPathSelectElement($"macro/connections/connection[@ref='{connName}']/macro")?.Attribute("ref")?.Value ?? "";
            if (string.IsNullOrEmpty(storage))
            {
                // カーゴが無い船(ゼノンの艦船等)を考慮
                return Enumerable.Empty<string>();
            }


            var storageXml = _CatFile.OpenIndexXml("index/macros.xml", storage);
            if (storageXml is null) return Enumerable.Empty<string>();

            var tags = storageXml.Root.XPathSelectElement("macro/properties/cargo")?.Attribute("tags")?.Value ?? "";

            return tags.Trim(' ').Split(" ");
        }
    }
}

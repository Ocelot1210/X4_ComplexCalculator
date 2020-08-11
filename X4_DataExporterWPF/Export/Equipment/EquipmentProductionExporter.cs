using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.Xml;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 装備作成時の情報抽出用クラス
    /// </summary>
    class EquipmentProductionExporter : IExporter
    {
        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waresXml">ウェア情報xml</param>
        public EquipmentProductionExporter(XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS EquipmentProduction
(
    EquipmentID TEXT    NOT NULL,
    Method      TEXT    NOT NULL,
    Time        REAL    NOT NULL,
    PRIMARY KEY (EquipmentID, Method),
    FOREIGN KEY (EquipmentID)   REFERENCES Equipment(EquipmentID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute("INSERT INTO EquipmentProduction (EquipmentID, Method, Time) VALUES (@EquipmentID, @Method, @Time)", items);
            }
        }


        /// <summary>
        /// XML から EquipmentProduction データを読み出す
        /// </summary>
        /// <returns>読み出した EquipmentProduction データ</returns>
        private IEnumerable<EquipmentProduction> GetRecords()
        {
            foreach (var equipment in _WaresXml.Root.XPathSelectElements("ware[@transport='equipment']"))
            {
                var equipmentID = equipment.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(equipmentID)) continue;

                foreach (var prod in equipment.XPathSelectElements("production"))
                {
                    var method = prod.Attribute("method")?.Value;
                    if (string.IsNullOrEmpty(method)) continue;

                    var time = prod.Attribute("time").GetDouble();
                    yield return new EquipmentProduction(equipmentID, method, time);
                }
            }
        }
    }
}

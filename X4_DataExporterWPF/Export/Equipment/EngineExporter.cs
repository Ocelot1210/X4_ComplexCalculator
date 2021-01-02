using Dapper;
using LibX4.FileSystem;
using LibX4.Xml;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// エンジン情報抽出用クラス
    /// </summary>
    class EngineExporter : IExporter
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
        public EngineExporter(IIndexResolver catFile, XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS Engine
(
    EquipmentID         TEXT    NOT NULL PRIMARY KEY,
    ForwardThrust       INTEGER NOT NULL,
    ReverseThrust       INTEGER NOT NULL,
    BoostThrust         INTEGER NOT NULL,
    BoostDuration       REAL    NOT NULL,
    BoostReleaseTime    REAL    NOT NULL,
    TravelThrust        INTEGER NOT NULL,
    TravelReleaseTime   REAL    NOT NULL,
    FOREIGN KEY (EquipmentID)   REFERENCES Equipment(EquipmentID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute(@"
INSERT INTO Engine ( EquipmentID,  ForwardThrust,  ReverseThrust,  BoostThrust,  BoostDuration,  BoostReleaseTime,  TravelThrust,  TravelReleaseTime,  TravelThrust,  TravelReleaseTime)
            VALUES (@EquipmentID, @ForwardThrust, @ReverseThrust, @BoostThrust, @BoostDuration, @BoostReleaseTime, @TravelThrust, @TravelReleaseTime, @TravelThrust, @TravelReleaseTime)", items);
            }
        }


        /// <summary>
        /// XML から Engine データを読み出す
        /// </summary>
        /// <returns>読み出した Engine データ</returns>
        private IEnumerable<Engine> GetRecords()
        {
            foreach (var equipment in _WaresXml.Root.XPathSelectElements("ware[@transport='equipment'][@group='engines']"))
            {
                var equipmentID = equipment.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(equipmentID)) continue;

                var macroName = equipment.XPathSelectElement("component")?.Attribute("ref")?.Value;
                if (string.IsNullOrEmpty(macroName)) continue;

                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);
                if (macroXml is null) continue;


                var thrust = macroXml.Root.XPathSelectElement("macro/properties/thrust");
                var boost = macroXml.Root.XPathSelectElement("macro/properties/boost");
                var travel = macroXml.Root.XPathSelectElement("macro/properties/travel");
                if (thrust is null || boost is null || travel is null) continue;

                var forwardThrust = thrust.Attribute("forward")?.GetInt() ?? 0;

                yield return new Engine(
                    equipmentID,
                    forwardThrust,
                    thrust.Attribute("reverse")?.GetInt() ?? 0,
                    (int)(forwardThrust * boost.Attribute("thrust")?.GetDouble() ?? 1.0),
                    boost.Attribute("duration")?.GetDouble() ?? 0.0,
                    boost.Attribute("release")?.GetDouble() ?? 0.0,
                    (int)(forwardThrust * travel.Attribute("thrust")?.GetDouble() ?? 1.0),
                    travel.Attribute("release")?.GetDouble() ?? 0.0);
            }
        }
    }
}

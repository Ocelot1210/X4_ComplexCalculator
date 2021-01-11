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
    /// スラスター情報抽出用クラス
    /// </summary>
    class ThrusterExporter : IExporter
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
        public ThrusterExporter(IIndexResolver catFile, XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS Thruster
(
    EquipmentID     TEXT    NOT NULL PRIMARY KEY,
    ThrustStrafe    REAL    NOT NULL,
    ThrustPitch     REAL    NOT NULL,
    ThrustYaw       REAL    NOT NULL,
    ThrustRoll      REAL    NOT NULL,
    AngularRoll     REAL    NOT NULL,
    AngularPitch    REAL    NOT NULL,
    FOREIGN KEY (EquipmentID)   REFERENCES Equipment(EquipmentID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute(@"
INSERT INTO Thruster ( EquipmentID,  ThrustStrafe,  ThrustPitch,  ThrustYaw,  ThrustRoll,  AngularRoll,  AngularPitch)
            VALUES   (@EquipmentID, @ThrustStrafe, @ThrustPitch, @ThrustYaw, @ThrustRoll, @AngularRoll, @AngularPitch)", items);
            }
        }


        /// <summary>
        /// XML から Engine データを読み出す
        /// </summary>
        /// <returns>読み出した Engine データ</returns>
        private IEnumerable<Thruster> GetRecords()
        {
            foreach (var equipment in _WaresXml.Root.XPathSelectElements("ware[@transport='equipment'][@group='thrusters']"))
            {
                var equipmentID = equipment.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(equipmentID)) continue;

                var macroName = equipment.XPathSelectElement("component")?.Attribute("ref")?.Value;
                if (string.IsNullOrEmpty(macroName)) continue;

                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);
                if (macroXml is null) continue;


                var thrust = macroXml.Root.XPathSelectElement("macro/properties/thrust");
                var angular = macroXml.Root.XPathSelectElement("macro/properties/angular");
                if (thrust is null || angular is null) continue;


                yield return new Thruster(
                    equipmentID,
                    thrust.Attribute("strafe")?.GetDouble() ?? 0.0,
                    thrust.Attribute("pitch")?.GetDouble()  ?? 0.0,
                    thrust.Attribute("yaw")?.GetDouble()    ?? 0.0,
                    thrust.Attribute("roll")?.GetDouble()   ?? 0.0,
                    angular.Attribute("roll")?.GetDouble()  ?? 0.0,
                    angular.Attribute("pitch")?.GetDouble() ?? 0.0
                );
            }
        }
    }
}

using Dapper;
using LibX4.FileSystem;
using LibX4.Xml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// シールド情報抽出用クラス
    /// </summary>
    class ShieldExporter : IExporter
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
        public ShieldExporter(IIndexResolver catFile, XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS Shield
(
    EquipmentID     TEXT    NOT NULL PRIMARY KEY,
    Capacity        INTEGER NOT NULL,
    RechargeRate    INTEGER NOT NULL,
    RechargeDelay   REAL    NOT NULL,
    FOREIGN KEY (EquipmentID)   REFERENCES Equipment(EquipmentID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords(progress);

                connection.Execute("INSERT INTO Shield (EquipmentID, Capacity, RechargeRate, RechargeDelay) VALUES (@EquipmentID, @Capacity, @RechargeRate, @RechargeDelay)", items);
            }
        }


        /// <summary>
        /// XML から Shield データを読み出す
        /// </summary>
        /// <returns>読み出した Shield データ</returns>
        private IEnumerable<Shield> GetRecords(IProgress<(int currentStep, int maxSteps)> progress)
        {
            var maxSteps = (int)(double)_WaresXml.Root.XPathEvaluate("count(ware[@transport='equipment'][@group='shields'])");
            var currentStep = 0;


            foreach (var equipment in _WaresXml.Root.XPathSelectElements("ware[@transport='equipment'][@group='shields']"))
            {
                progress.Report((currentStep++, maxSteps));


                var equipmentID = equipment.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(equipmentID)) continue;

                var macroName = equipment.XPathSelectElement("component")?.Attribute("ref")?.Value;
                if (string.IsNullOrEmpty(macroName)) continue;

                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);
                XDocument componentXml;
                try
                {
                    componentXml = _CatFile.OpenIndexXml("index/components.xml", macroXml.Root.XPathSelectElement("macro/component").Attribute("ref").Value);
                }
                catch
                {
                    continue;
                }

                var idTag = macroXml.Root.XPathSelectElement("macro/properties/identification");
                if (idTag is null) continue;

                var rechargeElm = macroXml.Root.XPathSelectElement("macro/properties/recharge");
                if (rechargeElm is null) continue;

                yield return new Shield(
                    equipmentID,
                    rechargeElm.Attribute("max")?.GetInt() ?? 0,
                    rechargeElm.Attribute("rate")?.GetInt() ?? 0,
                    rechargeElm.Attribute("delay")?.GetDouble() ?? 0.0);
            }

            progress.Report((currentStep++, maxSteps));
        }
    }
}

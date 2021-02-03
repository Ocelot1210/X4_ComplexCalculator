﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// ウェア所有派閥情報抽出用クラス
    /// </summary>
    public class WareOwnerExporter : IExporter
    {
        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waresXml">ウェア情報xml</param>
        public WareOwnerExporter(XDocument waresXml)
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
CREATE TABLE IF NOT EXISTS WareOwner
(
    WareID      TEXT    NOT NULL,
    FactionID   TEXT    NOT NULL,
    PRIMARY KEY (WareID, FactionID),
    FOREIGN KEY (WareID)  REFERENCES Ware(WareID),
    FOREIGN KEY (FactionID) REFERENCES Faction(FactionID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords(progress);


                connection.Execute("INSERT INTO WareOwner (WareID, FactionID) VALUES (@WareID, @FactionID)", items);
            }
        }


        /// <summary>
        /// XML から WareOwner データを読み出す
        /// </summary>
        /// <returns>読み出した WareOwner データ</returns>
        internal IEnumerable<WareOwner> GetRecords(IProgress<(int currentStep, int maxSteps)>? progress = null)
        {
            var maxSteps = (int)(double)_WaresXml.Root.XPathEvaluate("count(ware)");
            var currentStep = 0;


            foreach (var ware in _WaresXml.Root.XPathSelectElements("ware"))
            {
                progress?.Report((currentStep++, maxSteps));

                var wareID = ware.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(wareID)) continue;

                var owners = ware.XPathSelectElements("owner")
                    .Select(owner => owner.Attribute("faction")?.Value)
                    .Where(factionID => !string.IsNullOrEmpty(factionID))
                    .Select(x => x!)
                    .Distinct();

                foreach (var factionID in owners)
                {
                    yield return new WareOwner(wareID, factionID);
                }
            }

            progress?.Report((currentStep++, maxSteps));
        }
    }
}

using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.FileSystem;
using LibX4.Lang;
using X4_DataExporterWPF.Entity;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 派閥情報抽出用クラス
    /// </summary>
    public class FactionExporter : IExporter
    {
        /// <summary>
        /// catファイルオブジェクト
        /// </summary>
        private readonly CatFile _CatFile;


        /// <summary>
        /// 言語解決用オブジェクト
        /// </summary>
        private readonly ILanguageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public FactionExporter(CatFile catFile, ILanguageResolver resolver)
        {
            _CatFile = catFile;
            _Resolver = resolver;
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
CREATE TABLE IF NOT EXISTS Faction
(
    FactionID   TEXT    NOT NULL PRIMARY KEY,
    Name        TEXT    NOT NULL,
    RaceID      TEXT    NOT NULL,
    ShortName   TEXT    NOT NULL,
    Description TEXT    NOT NULL,
    Icon        BLOB,
    FOREIGN KEY (RaceID)   REFERENCES Race(RaceID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords(progress);

                connection.Execute("INSERT INTO Faction (FactionID, Name, RaceID, ShortName, Description, Icon) VALUES (@FactionID, @Name, @RaceID, @ShortName, @Description, @Icon)", items);
            }
        }


        /// <summary>
        /// XML から Faction データを読み出す
        /// </summary>
        /// <returns>読み出した Faction データ</returns>
        private IEnumerable<Faction> GetRecords(IProgress<(int currentStep, int maxSteps)> progress)
        {
            var factionsXml = _CatFile.OpenXml("libraries/factions.xml");

            var maxSteps = (int)(double)factionsXml.Root.XPathEvaluate("count(faction[@name])");
            var currentStep = 0;
            
            foreach (var faction in factionsXml.Root.XPathSelectElements("faction[@name]"))
            {
                progress.Report((currentStep++, maxSteps));

                var factionID = faction.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(factionID)) continue;

                var name = _Resolver.Resolve(faction.Attribute("name")?.Value ?? "");
                if (string.IsNullOrEmpty(name)) continue;

                var raceID = faction.Attribute("primaryrace")?.Value ?? "";
                var shortName = _Resolver.Resolve(faction.Attribute("shortname")?.Value ?? "");

                yield return new Faction(
                    factionID,
                    name,
                    raceID,
                    shortName,
                    _Resolver.Resolve(faction.Attribute("description")?.Value ?? ""),
                    Util.DDS2Png(_CatFile, "assets/fx/gui/textures/factions", faction.Element("icon")?.Attribute("active")?.Value)
                );
            }

            progress.Report((currentStep++, maxSteps));
        }
    }
}

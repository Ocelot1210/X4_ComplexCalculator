﻿using Dapper;
using LibX4.FileSystem;
using LibX4.Lang;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 種族情報抽出用クラス
    /// </summary>
    public class RaceExporter : IExporter
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
        /// <param name="raceXml">catファイルオブジェクト</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public RaceExporter(CatFile catFile, ILanguageResolver resolver)
        {
            _CatFile = catFile;
            _Resolver = resolver;
        }


        /// <summary>
        /// データ抽出
        /// </summary>
        /// <param name="connection"></param>
        public void Export(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress)
        {
            //////////////////
            // テーブル作成 //
            //////////////////
            {
                connection.Execute(@"
CREATE TABLE IF NOT EXISTS Race
(
    RaceID      TEXT    NOT NULL PRIMARY KEY,
    Name        TEXT    NOT NULL,
    ShortName   TEXT    NOT NULL,
    Description TEXT    NOT NULL,
    Icon        BLOB
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords(progress);

                connection.Execute("INSERT INTO Race (RaceID, Name, ShortName, Description, Icon) VALUES (@RaceID, @Name, @ShortName, @Description, @Icon)", items);
            }
        }


        /// <summary>
        /// XML から Race データを読み出す
        /// </summary>
        /// <returns>読み出した Race データ</returns>
        private IEnumerable<Race> GetRecords(IProgress<(int currentStep, int maxSteps)> progress)
        {
            var raceXml = _CatFile.OpenXml("libraries/races.xml");

            var maxSteps = raceXml.Root.Elements().Count();
            int currentStep = 0;

            foreach (var race in raceXml.Root.Elements())
            {
                progress.Report((currentStep++, maxSteps));

                var raceID = race.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(raceID)) continue;

                var name = _Resolver.Resolve(race.Attribute("name")?.Value ?? "");
                if (string.IsNullOrEmpty(name)) continue;

                var shortName = _Resolver.Resolve(race.Attribute("shortname")?.Value ?? "");
                
                var description = _Resolver.Resolve(race.Attribute("description")?.Value ?? "");

                yield return new Race(
                    raceID,
                    name,
                    shortName,
                    description,
                    Util.DDS2Png(_CatFile, "assets/fx/gui/textures/races", race.Element("icon")?.Attribute("active")?.Value)
                );
            }

            progress.Report((currentStep++, maxSteps));
        }
    }
}

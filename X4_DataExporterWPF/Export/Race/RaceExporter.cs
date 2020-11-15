using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using Dapper;
using LibX4.Lang;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 種族情報抽出用クラス
    /// </summary>
    public class RaceExporter : IExporter
    {
        /// <summary>
        /// 種族情報xml
        /// </summary>
        private readonly XDocument _RaceXml;


        /// <summary>
        /// 言語解決用オブジェクト
        /// </summary>
        private readonly ILanguageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="raceXml">'libraries/races.xml' の XDocument</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public RaceExporter(XDocument raceXml, ILanguageResolver resolver)
        {
            _RaceXml = raceXml;
            _Resolver = resolver;
        }


        /// <summary>
        /// データ抽出
        /// </summary>
        /// <param name="connection"></param>
        public void Export(IDbConnection connection)
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
    Description TEXT    NOT NULL
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute("INSERT INTO Race (RaceID, Name, ShortName, Description) VALUES (@RaceID, @Name, @ShortName, @Description)", items);
            }
        }


        /// <summary>
        /// XML から Race データを読み出す
        /// </summary>
        /// <returns>読み出した Race データ</returns>
        private IEnumerable<Race> GetRecords()
        {
            foreach (var race in _RaceXml.Root.Elements())
            {
                var raceID = race.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(raceID)) continue;

                var name = _Resolver.Resolve(race.Attribute("name")?.Value ?? "");
                if (string.IsNullOrEmpty(name)) continue;

                var shortName = _Resolver.Resolve(race.Attribute("shortname")?.Value ?? "");
                
                var description = _Resolver.Resolve(race.Attribute("description")?.Value ?? "");

                yield return new Race(raceID, name, shortName, description);
            }
        }
    }
}

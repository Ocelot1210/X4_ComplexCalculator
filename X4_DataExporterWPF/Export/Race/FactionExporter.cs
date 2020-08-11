using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.Lang;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 派閥情報抽出用クラス
    /// </summary>
    public class FactionExporter : IExporter
    {
        /// <summary>
        /// 派閥情報xml
        /// </summary>
        private readonly XDocument _FactionsXml;


        /// <summary>
        /// 言語解決用オブジェクト
        /// </summary>
        private readonly LanguageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="factionsXml">'libraries/factions.xml' の XDocument</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public FactionExporter(XDocument factionsXml, LanguageResolver resolver)
        {
            _FactionsXml = factionsXml;

            _Resolver = resolver;
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
CREATE TABLE IF NOT EXISTS Faction
(
    FactionID   TEXT    NOT NULL PRIMARY KEY,
    Name        TEXT    NOT NULL,
    RaceID      TEXT    NOT NULL,
    ShortName   TEXT    NOT NULL,
    FOREIGN KEY (RaceID)   REFERENCES Race(RaceID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute("INSERT INTO Faction (FactionID, Name, RaceID, ShortName) VALUES (@FactionID, @Name, @RaceID, @ShortName)", items);
            }
        }


        /// <summary>
        /// XML から Faction データを読み出す
        /// </summary>
        /// <returns>読み出した Faction データ</returns>
        private IEnumerable<Faction> GetRecords()
        {
            foreach (var faction in _FactionsXml.Root.XPathSelectElements("faction[@name]"))
            {
                var factionID = faction.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(factionID)) continue;

                var name = _Resolver.Resolve(faction.Attribute("name")?.Value ?? "");
                if (string.IsNullOrEmpty(name)) continue;

                var raceID = faction.Attribute("primaryrace")?.Value ?? "";
                var shortName = _Resolver.Resolve(faction.Attribute("shortname")?.Value ?? "");

                yield return new Faction(factionID, name, raceID, shortName);
            }
        }
    }
}

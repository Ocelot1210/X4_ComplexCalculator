using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.FileSystem;
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
        private readonly LangageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイル</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public FactionExporter(CatFile catFile, LangageResolver resolver)
        {
            _FactionsXml = catFile.OpenXml("libraries/factions.xml");

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
                var items = _FactionsXml.Root.XPathSelectElements("faction[@name]").Select
                (
                    x =>
                    {
                        var factionID = x.Attribute("id")?.Value;
                        if (string.IsNullOrEmpty(factionID)) return null;

                        var name = _Resolver.Resolve(x.Attribute("name")?.Value ?? "");
                        if (string.IsNullOrEmpty(name)) return null;

                        var raceID = x.Attribute("primaryrace")?.Value ?? "";
                        var shortName = _Resolver.Resolve(x.Attribute("shortname")?.Value ?? "");

                        return new Faction(factionID, name, raceID, shortName);
                    }
                )
                .Where
                (
                    x => x != null
                );

                connection.Execute("INSERT INTO Faction (FactionID, Name, RaceID, ShortName) VALUES (@FactionID, @Name, @RaceID, @ShortName)", items);
            }
        }
    }
}

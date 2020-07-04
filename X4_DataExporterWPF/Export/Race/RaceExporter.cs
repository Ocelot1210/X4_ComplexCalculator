using System.Data;
using System.Linq;
using System.Xml.Linq;
using Dapper;
using LibX4.FileSystem;
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
        private readonly LangageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイル</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public RaceExporter(CatFile catFile, LangageResolver resolver)
        {
            _RaceXml = catFile.OpenXml("libraries/races.xml");

            _Resolver = resolver;
        }


        /// <summary>
        /// データ抽出
        /// </summary>
        /// <param name="cmd"></param>
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
    ShortName   TEXT    NOT NULL
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = _RaceXml.Root.Elements().Select
                (
                    x =>
                    {
                        var raceID = x.Attribute("id")?.Value;
                        if (string.IsNullOrEmpty(raceID)) return null;

                        var name = _Resolver.Resolve(x.Attribute("name")?.Value ?? "");
                        if (string.IsNullOrEmpty(name)) return null;

                        var shortName = _Resolver.Resolve(x.Attribute("shortname")?.Value ?? "");

                        return new Race(raceID, name, shortName);
                    }
                )
                .Where
                (
                    x => x != null
                );

                connection.Execute("INSERT INTO Race (RaceID, Name, ShortName) VALUES (@RaceID, @Name, @ShortName)", items);
            }
        }
    }
}

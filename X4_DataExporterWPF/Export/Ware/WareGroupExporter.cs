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
    /// ウェア種別抽出用クラス
    /// </summary>
    public class WareGroupExporter : IExporter
    {
        /// <summary>
        /// ウェア種別情報xml
        /// </summary>
        private readonly XDocument _WareGroupXml;


        /// <summary>
        /// 言語解決用オブジェクト
        /// </summary>
        private readonly LangageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイル</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public WareGroupExporter(CatFile catFile, LangageResolver resolver)
        {
            _WareGroupXml = catFile.OpenXml("libraries/waregroups.xml");

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
CREATE TABLE IF NOT EXISTS WareGroup
(
    WareGroupID TEXT    NOT NULL PRIMARY KEY,
    Name        TEXT    NOT NULL,
    FactoryName TEXT    NOT NULL,
    Icon        TEXT    NOT NULL,
    Tier        INTEGER NOT NULL
) WITHOUT ROWID");
            }

            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = _WareGroupXml.Root.XPathSelectElements("group[@tags='tradable']").Select
                (
                    x =>
                    {
                        var wareGroupID = x.Attribute("id")?.Value;
                        if (string.IsNullOrEmpty(wareGroupID)) return null;

                        var name = _Resolver.Resolve(x.Attribute("name")?.Value ?? "");
                        if (string.IsNullOrEmpty(name)) return null;

                        var factoryName = _Resolver.Resolve(x.Attribute("factoryname")?.Value ?? "");
                        var icon = x.Attribute("icon")?.Value ?? "";
                        var tier = int.Parse(x.Attribute("tier")?.Value ?? "0");

                        return new WareGroup(wareGroupID, name, factoryName, icon, tier);
                    }
                )
                .Where
                (
                    x => x != null
                );

                connection.Execute("INSERT INTO WareGroup (WareGroupID, Name, FactoryName, Icon, Tier) VALUES (@WareGroupID, @Name, @FactoryName, @Icon, @Tier)", items);
            }
        }
    }
}

using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.Lang;
using LibX4.Xml;
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
        private readonly ILanguageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareGroupXml">'libraries/waregroups.xml' の XDocument</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public WareGroupExporter(XDocument wareGroupXml, ILanguageResolver resolver)
        {
            _WareGroupXml = wareGroupXml;
            _Resolver = resolver;
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
                var items = GetRecords();

                connection.Execute("INSERT INTO WareGroup (WareGroupID, Name, FactoryName, Icon, Tier) VALUES (@WareGroupID, @Name, @FactoryName, @Icon, @Tier)", items);
            }
        }


        /// <summary>
        /// XML から WareGroup データを読み出す
        /// </summary>
        /// <returns>読み出した WareGroup データ</returns>
        internal IEnumerable<WareGroup> GetRecords()
        {
            foreach (var wareGroup in _WareGroupXml.Root.XPathSelectElements("group[@tags='tradable']"))
            {
                var wareGroupID = wareGroup.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(wareGroupID)) continue;

                var name = _Resolver.Resolve(wareGroup.Attribute("name")?.Value ?? "");
                if (string.IsNullOrEmpty(name)) continue;

                var factoryName = _Resolver.Resolve(wareGroup.Attribute("factoryname")?.Value ?? "");
                var icon = wareGroup.Attribute("icon")?.Value ?? "";
                var tier = wareGroup.Attribute("tier")?.GetInt() ?? 0;

                yield return new WareGroup(wareGroupID, name, factoryName, icon, tier);
            }
        }
    }
}

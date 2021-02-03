using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.FileSystem;
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
        /// catファイルオブジェクト
        /// </summary>
        private readonly ICatFile _CatFile;


        /// <summary>
        /// 言語解決用オブジェクト
        /// </summary>
        private readonly ILanguageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareGroupXml">'libraries/waregroups.xml' の XDocument</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public WareGroupExporter(ICatFile catFile, ILanguageResolver resolver)
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
CREATE TABLE IF NOT EXISTS WareGroup
(
    WareGroupID TEXT    NOT NULL PRIMARY KEY,
    Name        TEXT    NOT NULL,
    Tier        INTEGER NOT NULL
) WITHOUT ROWID");
            }

            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords(progress);

                connection.Execute("INSERT INTO WareGroup (WareGroupID, Name, Tier) VALUES (@WareGroupID, @Name, @Tier)", items);
            }
        }


        /// <summary>
        /// XML から WareGroup データを読み出す
        /// </summary>
        /// <returns>読み出した WareGroup データ</returns>
        internal IEnumerable<WareGroup> GetRecords(IProgress<(int currentStep, int maxSteps)>? progress = null)
        {
            var wareGroupXml = _CatFile.OpenXml("libraries/waregroups.xml");

            var maxSteps = (int)(double)wareGroupXml.Root.XPathEvaluate("count(group)");
            var currentStep = 0;
            
            foreach (var wareGroup in wareGroupXml.Root.XPathSelectElements("group"))
            {
                progress?.Report((currentStep++, maxSteps));

                var wareGroupID = wareGroup.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(wareGroupID)) continue;

                var name = _Resolver.Resolve(wareGroup.Attribute("name")?.Value ?? "");

                var tier = wareGroup.Attribute("tier")?.GetInt() ?? 0;

                yield return new WareGroup(wareGroupID, name, tier);
            }

            progress?.Report((currentStep++, maxSteps));
        }
    }
}

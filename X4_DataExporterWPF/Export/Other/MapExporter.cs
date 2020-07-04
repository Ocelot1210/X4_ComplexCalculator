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
    class MapExporter : IExporter
    {
        /// <summary>
        /// マップ情報xml
        /// </summary>
        private readonly XDocument _MapXml;


        /// <summary>
        /// 言語解決用オブジェクト
        /// </summary>
        private readonly LangageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイル</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public MapExporter(CatFile catFile, LangageResolver resolver)
        {
            _MapXml = catFile.OpenXml("libraries/mapdefaults.xml");
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
CREATE TABLE IF NOT EXISTS Map
(
    Macro       TEXT    NOT NULL PRIMARY KEY,
    Name        TEXT    NOT NULL,
    Description TEXT    NOT NULL
)");
            }



            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = _MapXml.Root.XPathSelectElements("dataset[not(starts-with(@macro, 'demo'))]/properties/identification/../..").Select
                (
                    dataset =>
                    {
                        var macro = dataset.Attribute("macro").Value;

                        var id = dataset.XPathSelectElement("properties/identification");

                        return new Map(
                            macro,
                            _Resolver.Resolve(id.Attribute("name").Value),
                            _Resolver.Resolve(id.Attribute("description").Value)
                        );
                    }
                );


                connection.Execute("INSERT INTO Map (Macro, Name, Description) VALUES (@Macro, @Name, @Description)", items);
            }


            ///////////////
            // Index作成 //
            ///////////////
            {
                connection.Execute("CREATE INDEX MapIndex ON Ware(Macro)");
            }
        }
    }
}

using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
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
        private readonly ILanguageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mapXml">'libraries/mapdefaults.xml' の XDocument</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public MapExporter(XDocument mapXml, ILanguageResolver resolver)
        {
            _MapXml = mapXml;
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
                var items = GetRecords();


                connection.Execute("INSERT INTO Map (Macro, Name, Description) VALUES (@Macro, @Name, @Description)", items);
            }


            ///////////////
            // Index作成 //
            ///////////////
            {
                connection.Execute("CREATE INDEX MapIndex ON Ware(Macro)");
            }
        }


        /// <summary>
        /// XML から Map データを読み出す
        /// </summary>
        /// <returns>読み出した Map データ</returns>
        private IEnumerable<Map> GetRecords()
        {
            foreach (var dataset in _MapXml.Root.XPathSelectElements("dataset[not(starts-with(@macro, 'demo'))]/properties/identification/../.."))
            {
                var macro = dataset.Attribute("macro").Value;

                var id = dataset.XPathSelectElement("properties/identification");

                var name =_Resolver.Resolve(id.Attribute("name").Value);
                var description = _Resolver.Resolve(id.Attribute("description").Value);

                yield return new Map(macro, name, description);
            }
        }
    }
}

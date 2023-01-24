using Dapper;
using LibX4.Lang;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;
using X4_DataExporterWPF.Internal;

namespace X4_DataExporterWPF.Export;

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


    /// <inheritdoc/>
    public async Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            await connection.ExecuteAsync(@"
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


            await connection.ExecuteAsync("INSERT INTO Map (Macro, Name, Description) VALUES (@Macro, @Name, @Description)", items);

            await connection.ExecuteAsync("CREATE INDEX MapIndex ON Ware(Macro)");
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

            var name = _Resolver.Resolve(id.Attribute("name").Value);
            var description = _Resolver.Resolve(id.Attribute("description").Value);

            yield return new Map(macro, name, description);
        }
    }
}
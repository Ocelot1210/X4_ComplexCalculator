using Dapper;
using LibX4.Lang;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;
using X4_DataExporterWPF.Internal;

namespace X4_DataExporterWPF.Export;

/// <summary>
/// カーゴ種別抽出用クラス
/// </summary>
class TransportTypeExporter : IExporter
{
    /// <summary>
    /// ウェア情報xml
    /// </summary>
    private readonly XDocument _WaresXml;


    /// <summary>
    /// 言語解決用オブジェクト
    /// </summary>
    private readonly ILanguageResolver _Resolver;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="resolver">言語解決用オブジェクト</param>
    public TransportTypeExporter(XDocument waresXml, ILanguageResolver resolver)
    {
        _WaresXml = waresXml;
        _Resolver = resolver;
    }


    /// <summary>
    /// 抽出処理
    /// </summary>
    /// <param name="connection"></param>
    public async Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            await connection.ExecuteAsync(@"
CREATE TABLE IF NOT EXISTS TransportType
(
    TransportTypeID TEXT    NOT NULL PRIMARY KEY,
    Name            TEXT    NOT NULL
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecords(progress, cancellationToken);

            // レコード追加
            await connection.ExecuteAsync("INSERT INTO TransportType (TransportTypeID, Name) VALUES (@TransportTypeID, @Name)", items);
        }
    }


    /// <summary>
    /// XML から TransportType データを読み出す
    /// </summary>
    /// <returns>読み出した TransportType データ</returns>
    private IEnumerable<TransportType> GetRecords(IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        // TODO: 可能ならファイルから抽出する
        var names = new Dictionary<string, string>()
        {
            {"container",  "{20205,  100}"},
            {"solid",      "{20205,  200}"},
            {"liquid",     "{20205,  300}"},
            {"passenger",  "{20205,  400}"},
            {"equipment",  "{20205,  500}"},
            {"inventory",  "{20205,  600}"},
            {"software",   "{20205,  700}"},
            {"workunit",   "{20205,  800}"},
            {"ship",       "{20205,  900}"},
            {"research",   "{20205, 1000}"},
            {"condensate", "{20205, 1100}"},
        };

        var maxSteps = (int)(double)_WaresXml.Root.XPathEvaluate("count(ware)");
        var currentStep = 0;

        var added = new HashSet<string>();
        foreach (var ware in _WaresXml.Root.Elements("ware"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report((currentStep++, maxSteps));

            var transportTypeID = ware.Attribute("transport")?.Value;
            if (string.IsNullOrEmpty(transportTypeID) || added.Contains(transportTypeID)) continue;

            var name = transportTypeID;
            if (names.TryGetValue(transportTypeID, out var nameID))
            {
                name = _Resolver.Resolve(nameID);
            }

            yield return new TransportType(transportTypeID, name);
            added.Add(transportTypeID);
        }

        progress.Report((currentStep++, maxSteps));

        foreach (var key in names.Keys.Except(added))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new TransportType(key, _Resolver.Resolve(names[key]));
        }
    }
}

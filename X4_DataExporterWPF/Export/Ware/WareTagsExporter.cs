using Dapper;
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
/// ウェアのタグ情報を抽出するクラス
/// </summary>
class WareTagsExporter : IExporter
{
    /// <summary>
    /// ウェア情報xml
    /// </summary>
    private readonly XDocument _waresXml;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="waresXml">ウェア情報xml</param>
    public WareTagsExporter(XDocument waresXml)
    {
        ArgumentNullException.ThrowIfNull(waresXml.Root);
        _waresXml = waresXml;
    }


    /// <inheritdoc/>
    public async Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            await connection.ExecuteAsync(@"
CREATE TABLE IF NOT EXISTS WareTags
(
    WareID  TEXT    NOT NULL,
    Tag     TEXT    NOT NULL,
    PRIMARY KEY (WareID, Tag),
    FOREIGN KEY (WareID)   REFERENCES Ware(WareID)
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecords(progress);

            await connection.ExecuteAsync("INSERT INTO WareTags (WareID, Tag) VALUES (@WareID, @Tag)", items);
        }
    }



    private IEnumerable<WareTag> GetRecords(IProgress<(int currentStep, int maxSteps)> progress)
    {
        var maxSteps = (int)(double)_waresXml.Root!.XPathEvaluate("count(ware)");
        var currentStep = 0;


        foreach (var ware in _waresXml.Root!.XPathSelectElements("ware"))
        {
            progress.Report((currentStep++, maxSteps));

            var wareID = ware.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(wareID)) continue;

            var tags = Util.SplitTags(ware.Attribute("tags")?.Value).Distinct();

            foreach (var tag in tags)
            {
                yield return new WareTag(wareID, tag);
            }
        }

        progress.Report((currentStep++, maxSteps));
    }
}

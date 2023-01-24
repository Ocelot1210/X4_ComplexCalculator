using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using X4_DataExporterWPF.Entity;
using X4_DataExporterWPF.Internal;

namespace X4_DataExporterWPF.Export;

/// <summary>
/// DBメタ情報抽出用クラス
/// </summary>
public class CommonExporter : IExporter
{
    /// <summary>
    /// 現在のDBフォーマットバージョン
    /// </summary>
    public const int CURRENT_FORMAT_VERSION = 4;


    /// <inheritdoc/>
    public async Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            // テーブル作成
            await connection.ExecuteAsync(@"
CREATE TABLE IF NOT EXISTS Common
(
    Item    TEXT    NOT NULL PRIMARY KEY,
    Value   INTEGER
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecords(progress, cancellationToken);

            await connection.ExecuteAsync($"INSERT INTO Common (Item, Value) VALUES (@Item, @Value)", items);
        }
    }


    /// <summary>
    /// Common データを返す
    /// </summary>
    /// <returns>Common データ</returns>
    private IEnumerable<Common> GetRecords(IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        (string item, int value)[] data =
        {
            ("FormatVersion", CURRENT_FORMAT_VERSION)
        };

        int currentStep = 0;
        progress.Report((currentStep++, data.Length));

        foreach (var (item, value) in data)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new Common(item, value);
            progress.Report((currentStep++, data.Length));
        }
    }
}

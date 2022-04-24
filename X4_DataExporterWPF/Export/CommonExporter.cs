using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export;

/// <summary>
/// ウェア生産時の追加効果抽出用クラス
/// </summary>
public class CommonExporter : IExporter
{
    /// <summary>
    /// 現在のDBフォーマットバージョン
    /// </summary>
    public const int CURRENT_FORMAT_VERSION = 3;


    public void Export(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            // テーブル作成
            connection.Execute(@"
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
            var items = GetRecords(progress);

            connection.Execute($"INSERT INTO Common (Item, Value) VALUES (@Item, @Value)", items);
        }
    }


    /// <summary>
    /// Common データを返す
    /// </summary>
    /// <returns>Common データ</returns>
    private IEnumerable<Common> GetRecords(IProgress<(int currentStep, int maxSteps)> progress)
    {
        (string item, int value)[] data =
        {
            ("FormatVersion", CURRENT_FORMAT_VERSION)
        };

        int currentStep = 0;
        progress.Report((currentStep++, data.Length));

        foreach (var (item, value) in data)
        {
            yield return new Common(item, value);
            progress.Report((currentStep++, data.Length));
        }
    }
}

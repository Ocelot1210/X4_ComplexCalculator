using Dapper;
using LibX4.Lang;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using X4_DataExporterWPF.Entity;
using X4_DataExporterWPF.Internal;

namespace X4_DataExporterWPF.Export;

/// <summary>
/// サイズ情報抽出用クラス
/// </summary>
class SizeExporter : IExporter
{
    /// <summary>
    /// 言語解決用オブジェクト
    /// </summary>
    private readonly ILanguageResolver _resolver;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="resolver">言語解決用オブジェクト</param>
    public SizeExporter(ILanguageResolver resolver)
    {
        _resolver = resolver;
    }


    /// <inheritdoc/>
    public async Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            await connection.ExecuteAsync(@"
CREATE TABLE IF NOT EXISTS Size
(
    SizeID  TEXT    NOT NULL PRIMARY KEY,
    Name    TEXT    NOT NULL
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecords(progress, cancellationToken);

            await connection.ExecuteAsync("INSERT INTO Size (SizeID, Name) VALUES (@SizeID, @Name)", items);
        }
    }


    /// <summary>
    /// XML から Size データを読み出す
    /// </summary>
    /// <returns>読み出した Size データ</returns>
    private IEnumerable<Size> GetRecords(IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        // TODO: 可能ならファイルから抽出する
        (string id, string name)[] data =
        {
            ("extrasmall",  "{1001, 52}"),
            ("small",       "{1001, 51}"),
            ("medium",      "{1001, 50}"),
            ("large",       "{1001, 49}"),
            ("extralarge",  "{1001, 48}"),
        };

        int currentStep = 0;
        progress.Report((currentStep++, data.Length));
        foreach (var (id, name) in data)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new Size(id, _resolver.Resolve(name));
            progress.Report((currentStep++, data.Length));
        }
    }
}

using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using X4_DataExporterWPF.Entity;
using System.Threading.Tasks;
using System.Threading;
using X4_DataExporterWPF.Internal;

namespace X4_DataExporterWPF.Export;

/// <summary>
/// ウェア生産時の追加効果抽出用クラス
/// </summary>
public class EffectExporter : IExporter
{
    /// <inheritdoc/>
    public async Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            // テーブル作成
            await connection.ExecuteAsync(@"
CREATE TABLE IF NOT EXISTS Effect
(
    EffectID    TEXT    NOT NULL PRIMARY KEY,
    Name        TEXT    NOT NULL
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecords(progress, cancellationToken);

            // レコード追加
            await connection.ExecuteAsync("INSERT INTO Effect (EffectID, Name) VALUES (@EffectID, @Name)", items);
        }
    }


    /// <summary>
    /// Effect データを読み出す
    /// </summary>
    /// <returns>EquipmentType データ</returns>
    private static IEnumerable<Effect> GetRecords(IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        // TODO: 可能ならファイルから抽出する
        (string id, string name)[] data =
        {
            ("work",        "work"),
            ("sunlight",    "sunlight"),
        };

        int currentStep = 0;
        progress.Report((currentStep++, data.Length));
        foreach (var (id, name) in data)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new Effect(id, name);
            progress.Report((currentStep++, data.Length));
        }
    }
}

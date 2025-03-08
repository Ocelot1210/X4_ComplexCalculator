using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace X4_DataExporterWPF.Export;

/// <summary>
/// Export処理用インターフェイス
/// </summary>
interface IExporter
{
    /// <summary>
    /// エクスポート処理
    /// </summary>
    /// <param name="connection">DB接続情報</param>
    /// <param name="progress">進捗</param>
    /// <param name="cancellationToken">キャンセル トークン</param>
    /// <returns></returns>
    Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken);
}

using Onova;
using Onova.Services;
using Reactive.Bindings;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace X4_ComplexCalculator.Infrastructure;

/// <summary>
/// アプリケーションの更新を行うクラス
/// </summary>
public class ApplicationUpdater
{
    #region スタティックメンバ
    /// <summary>
    /// 参照する GitHub のリポジトリのオーナー名
    /// </summary>
    private const string REPO_OWNER = "Ocelot1210";


    /// <summary>
    /// 参照する GitHub のリポジトリ名
    /// </summary>
    private const string REPO_NAME = "X4_ComplexCalculator";


    /// <summary>
    /// ダウンロードするアセットファイル名を表す簡易 glob パターン
    /// </summary>
    private const string ASSET_NAME = "X4_ComplexCalculator.zip";


    /// <summary>
    /// Onova で更新情報を GitHub リリースから取得するリゾルバ
    /// </summary>
    private static readonly GithubPackageResolver _Resolver = new(REPO_OWNER, REPO_NAME, ASSET_NAME);
    #endregion


    #region メンバ
    /// <summary>
    /// Onova のアップデーター
    /// </summary>
    private readonly UpdateManager _manager;


    /// <summary>
    /// 最後に確認した時点での最新バージョン。既に最新の場合は null
    /// </summary>
    private Version? _lastVersion;


    /// <summary>
    /// ダウンロードが完了している場合
    /// </summary>
    private Task? _downloadTask;


    /// <summary>
    /// ダウンロードの進捗
    /// </summary>
    private readonly ReactivePropertySlim<double> _downloadProgress = new();


    /// <summary>
    /// キャンセルトークン
    /// </summary>
    private readonly CancellationTokenSource _cancellation = new();
    #endregion


    #region プロパティ
    /// <summary>
    /// 更新をダウンロード中の場合は true
    /// </summary>
    public bool NowDownloading => _downloadTask is not null;


    /// <summary>
    /// 更新のダウンロードが正常終了した場合 true
    /// </summary>
    public bool FinishedDownload => _downloadTask?.IsCompletedSuccessfully ?? false;


    /// <summary>
    /// ダウンロードの進捗
    /// </summary>
    public IReadOnlyReactiveProperty<double> DownloadProgress => _downloadProgress;
    #endregion


    /// <summary>
    /// アップデーターを初期化する
    /// </summary>
    public ApplicationUpdater()
        => _manager = new UpdateManager(_Resolver, new ZipExcerptPackageExtractor());


    /// <summary>
    /// アップデートを確認する
    /// </summary>
    /// <returns>更新がある場合はバージョン名、ない場合は null</returns>
    public async ValueTask<string?> CheckUpdate()
    {
        var result = await _manager.CheckForUpdatesAsync();
        return result.CanUpdate && result.LastVersion is not null
            ? (_lastVersion = result.LastVersion).ToString()
            : null;
    }


    /// <summary>
    /// 最新バージョンをバックグラウンドでダウンロードする
    /// </summary>
    public void StartDownloadByBackground()
    {
        var version = _lastVersion ?? throw new InvalidOperationException();
        var progless = new Progress<double>(progress => _downloadProgress.Value = progress);
        _downloadTask = _manager.PrepareUpdateAsync(version, progless, _cancellation.Token);
    }


    /// <summary>
    /// ダウンロードをキャンセルする
    /// </summary>
    public void CancelDownload() => _cancellation.Cancel();


    /// <summary>
    /// ダウンロードが終わり次第、アップデートの適用とアプリケーションの再起動を行う。
    /// ダウンロードしていない場合は何もしない
    /// </summary>
    public async ValueTask UpdateAfterDownloading()
    {
        if (_downloadTask is null) return;
        await _downloadTask.ConfigureAwait(false);
        Update();
    }


    /// <summary>
    /// アップデートの適用とアプリケーションの再起動を行う
    /// </summary>
    [DoesNotReturn]
    public void Update()
    {
        var version = _lastVersion ?? throw new InvalidOperationException();
        _manager.LaunchUpdater(version, restart: false);
        Environment.Exit(0);
    }
}

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Onova;
using Onova.Services;
using Reactive.Bindings;

namespace X4_ComplexCalculator.Infrastructure
{
    /// <summary>
    /// アプリケーションの更新を行うクラス
    /// </summary>
    public class ApplicationUpdater
    {
        #region スタティックメンバ
        /// <summary>
        /// 参照する GitHub のリポジトリのオーナー名
        /// </summary>
        private const string _RepoOwner = "Ocelot1210";


        /// <summary>
        /// 参照する GitHub のリポジトリ名
        /// </summary>
        private const string _RepoName = "X4_ComplexCalculator";


        /// <summary>
        /// ダウンロードするアセットファイル名を表す簡易 glob パターン
        /// </summary>
        private const string _AssetName = "X4_ComplexCalculator.zip";


        /// <summary>
        /// Onova で更新情報を GitHub リリースから取得するリゾルバ
        /// </summary>
        private static readonly GithubPackageResolver _Resolver
            = new GithubPackageResolver(_RepoOwner, _RepoName, _AssetName);
        #endregion


        #region メンバ
        /// <summary>
        /// Onova のアップデーター
        /// </summary>
        private readonly UpdateManager _Manager;


        /// <summary>
        /// 最後に確認した時点での最新バージョン。既に最新の場合は null
        /// </summary>
        private Version? _LastVersion;


        /// <summary>
        /// ダウンロードが完了している場合
        /// </summary>
        private Task? _DownloadTask;


        /// <summary>
        /// ダウンロードの進捗
        /// </summary>
        private ReactivePropertySlim<double> _DownloadProgress
            = new ReactivePropertySlim<double>();


        /// <summary>
        /// キャンセルトークン
        /// </summary>
        private CancellationTokenSource _Cancellation = new CancellationTokenSource();
        #endregion


        #region プロパティ
        /// <summary>
        /// 更新をダウンロード中の場合は true
        /// </summary>
        public bool NowDownloading => _DownloadTask != null;


        /// <summary>
        /// 更新のダウンロードが正常終了した場合 true
        /// </summary>
        public bool FinishedDownload => _DownloadTask?.IsCompletedSuccessfully ?? false;


        /// <summary>
        /// ダウンロードの進捗
        /// </summary>
        public IReadOnlyReactiveProperty<double> DownloadProgress => _DownloadProgress;
        #endregion


        /// <summary>
        /// アップデーターを初期化する
        /// </summary>
        public ApplicationUpdater()
            => _Manager = new UpdateManager(_Resolver, new ZipPackageExtractor());


        /// <summary>
        /// アップデートを確認する
        /// </summary>
        /// <returns>更新がある場合はバージョン名、ない場合は null</returns>
        public async ValueTask<string?> CheckUpdate()
            => (_LastVersion = (await _Manager.CheckForUpdatesAsync()).LastVersion)?.ToString();


        /// <summary>
        /// 最新バージョンをバックグラウンドでダウンロードする
        /// </summary>
        public void StartDownloadByBackground()
        {
            var version = _LastVersion ?? throw new InvalidOperationException();
            var progless = new Progress<double>(progress => _DownloadProgress.Value = progress);
            _DownloadTask = _Manager.PrepareUpdateAsync(version, progless, _Cancellation.Token);
        }


        /// <summary>
        /// ダウンロードをキャンセルする
        /// </summary>
        public void CancelDownload() => _Cancellation.Cancel();


        /// <summary>
        /// ダウンロードが終わり次第、アップデートの適用とアプリケーションの再起動を行う。
        /// ダウンロードしていない場合は何もしない
        /// </summary>
        public async ValueTask UpdateAfterDownloading()
        {
            if (_DownloadTask == null) return;
            await _DownloadTask.ConfigureAwait(false);
            Update();
        }


        /// <summary>
        /// アップデートの適用とアプリケーションの再起動を行う
        /// </summary>
        [DoesNotReturn]
        public void Update()
        {
            var version = _LastVersion ?? throw new InvalidOperationException();
            _Manager.LaunchUpdater(version, restart: false);
            Environment.Exit(0);
        }
    }
}

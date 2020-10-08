using Prism.Mvvm;
using Reactive.Bindings;
using X4_ComplexCalculator.Infrastructure;

namespace X4_ComplexCalculator.Main
{
    /// <summary>
    /// アップデートのダウンロード進捗表示ビューモデル
    /// </summary>
    public class UpdateDownloadProgressViewModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// アップデート機能
        /// </summary>
        private readonly ApplicationUpdater _ApplicationUpdater;
        #endregion


        #region プロパティ
        /// <summary>
        /// ダウンロード状況
        /// </summary>
        public IReadOnlyReactiveProperty<double> DownloadProgress
            => _ApplicationUpdater.DownloadProgress;


        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        public ReactiveCommand CancelCommand { get; }
        #endregion


        /// <summary>
        /// アップデートのダウンロード進捗表示ビューモデルを初期化する
        /// </summary>
        /// <param name="applicationUpdater">アップデート機能</param>
        public UpdateDownloadProgressViewModel(ApplicationUpdater applicationUpdater)
        {
            _ApplicationUpdater = applicationUpdater;
            CancelCommand = new ReactiveCommand().WithSubscribe(Cancel);

            // ダウンロードが終わり次第アプリケーションを終了し、更新を適用する
            _ = applicationUpdater.UpdateAfterDownloading();
        }


        /// <summary>
        /// ダウンロードをキャンセルする
        /// </summary>
        private void Cancel() => _ApplicationUpdater.CancelDownload();
    }
}

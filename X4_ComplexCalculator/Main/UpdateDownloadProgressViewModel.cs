using CommunityToolkit.Mvvm.ComponentModel;
using Reactive.Bindings;
using X4_ComplexCalculator.Infrastructures;

namespace X4_ComplexCalculator.Main;

/// <summary>
/// アップデートのダウンロード進捗表示ビューモデル
/// </summary>
public sealed partial class UpdateDownloadProgressViewModel : ObservableObject
{
    #region メンバ
    /// <summary>
    /// アップデート機能
    /// </summary>
    private readonly ApplicationUpdater _applicationUpdater;
    #endregion


    #region プロパティ
    /// <summary>
    /// ダウンロード状況
    /// </summary>
    public IReadOnlyReactiveProperty<double> DownloadProgress
        => _applicationUpdater.DownloadProgress;


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
        _applicationUpdater = applicationUpdater;
        CancelCommand = new ReactiveCommand().WithSubscribe(Cancel);

        // ダウンロードが終わり次第アプリケーションを終了し、更新を適用する
#pragma warning disable CA2012 // ValueTask を正しく使用する必要があります
        _ = applicationUpdater.UpdateAfterDownloading();
#pragma warning restore CA2012 // ValueTask を正しく使用する必要があります
    }


    /// <summary>
    /// ダウンロードをキャンセルする
    /// </summary>
    private void Cancel() => _applicationUpdater.CancelDownload();
}

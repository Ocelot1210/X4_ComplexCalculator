using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Infrastructures;

namespace X4_ComplexCalculator.Main;

/// <summary>
/// アップデートのダウンロード進捗表示ビューモデル
/// </summary>
public sealed partial class UpdateDownloadProgressViewModel : ObservableRecipient
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
    public double DownloadProgress
        => _applicationUpdater.DownloadProgress;
    #endregion


    /// <summary>
    /// アップデートのダウンロード進捗表示ビューモデルを初期化する
    /// </summary>
    /// <param name="applicationUpdater">アップデート機能</param>
    public UpdateDownloadProgressViewModel(ApplicationUpdater applicationUpdater)
    {
        _applicationUpdater = applicationUpdater;

        WeakReferenceMessenger.Default.RegisterPropertyChangedMessage(this, static (ApplicationUpdater x) => x.DownloadProgress, static (r, m) => r.OnPropertyChanged(nameof(DownloadProgress)));

        // ダウンロードが終わり次第アプリケーションを終了し、更新を適用する
#pragma warning disable CA2012 // ValueTask を正しく使用する必要があります
        _ = applicationUpdater.UpdateAfterDownloading();
#pragma warning restore CA2012 // ValueTask を正しく使用する必要があります
    }


    /// <summary>
    /// ダウンロードをキャンセルする
    /// </summary>
    [RelayCommand]
    private void Cancel() => _applicationUpdater.CancelDownload();
}

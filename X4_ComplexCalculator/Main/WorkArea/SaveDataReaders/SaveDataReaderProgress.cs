using CommunityToolkit.Mvvm.ComponentModel;
using System;
using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReaders;


/// <summary>
/// 保存ファイル読み込み時の進捗表示用
/// </summary>
internal sealed partial class SaveDataReaderProgress : ObservableObject
{
    #region メンバ
    /// <summary>
    /// 読み込み済みファイル数
    /// </summary>
    private int _loaded;
    #endregion


    #region プロパティ
    /// <summary>
    /// ビジー状態か
    /// </summary>
    [ObservableProperty]
    public partial bool IsBusy { get; set; }


    /// <summary>
    /// 進捗最大値
    /// </summary>
    [ObservableProperty]
    public partial double Maximum { get; private set; }


    /// <summary>
    /// ファイル読み込み進捗
    /// </summary>
    [ObservableProperty]
    public partial double Value { get; set; }


    /// <summary>
    /// 読込中のファイル名
    /// </summary>
    [ObservableProperty]
    public partial string LoadingFileName { get; set; } = "";


    /// <summary>
    /// 進捗
    /// </summary>
    public IProgress<double> Progress { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public SaveDataReaderProgress()
    {
        var progress = new Progress<double>();
        progress.ProgressChanged += OnProgressChanged;
        Progress = progress;
    }


    /// <summary>
    /// 読み込み予定のファイル数
    /// </summary>
    public void Init(int filesCount)
    {
        _loaded = 0;
        Value = 0;
        Maximum  = filesCount * 100;
    }



    public IDisposable Start(int filesCount)
    {
        _loaded = 0;
        Value = 0;
        Maximum = filesCount * 100;
        IsBusy = true;

        return new EasyDisposable()
        {
            DisposeAction = () =>
            {
                IsBusy = false;
                Value = 0;
            }
        };
    }


    /// <summary>
    /// 1ファイル読み込み完了時
    /// </summary>
    public void OnFileLoaded()
    {
        _loaded++;
        Value = (_loaded * 100);
    }


    /// <summary>
    /// 進捗変更時
    /// </summary>
    private void OnProgressChanged(object? sender, double e)
    {
        Value = (_loaded * 100) + e;
    }
}

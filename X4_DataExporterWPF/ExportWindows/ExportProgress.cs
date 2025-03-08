using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace X4_DataExporterWPF.DataExportWindows;

/// <summary>
/// データ抽出進捗
/// </summary>
sealed partial class ExportProgress : ObservableObject
{
    /// <summary>
    /// 進捗最大
    /// </summary>
    [ObservableProperty]
    public partial int MaxSteps { get; private set; } = 1;


    /// <summary>
    /// 現在の進捗
    /// </summary>
    [ObservableProperty]
    public partial int CurrentStep { get; private set; } = 0;


    /// <summary>
    /// 進捗最大(小項目)
    /// </summary>
    [ObservableProperty]
    public partial int MaxStepsSub { get; private set; } = 1;


    /// <summary>
    /// 現在の進捗(小項目)
    /// </summary>
    [ObservableProperty]
    public partial int CurrentStepSub { get; private set; } = 0;


    /// <summary>
    /// メイン処理進捗
    /// </summary>
    public Progress<(int currentStep, int maxSteps)> MainProgress { get; }


    /// <summary>
    /// サブ処理進捗
    /// </summary>
    public Progress<(int currentStep, int maxSteps)> SubProgress { get; }


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public ExportProgress()
    {
        MainProgress = new(OnMainProgressChanged);
        SubProgress = new(OnSubProgressChanged);
    }


    /// <summary>
    /// メイン処理進捗変更時
    /// </summary>
    private void OnMainProgressChanged((int currentStep, int maxSteps) progress)
    {
        CurrentStep = progress.currentStep;
        MaxSteps = progress.maxSteps;
    }


    /// <summary>
    /// サブ処理進捗変更時
    /// </summary>
    private void OnSubProgressChanged((int currentStep, int maxSteps) progress)
    {
        CurrentStepSub = progress.currentStep;
        MaxStepsSub = progress.maxSteps;
    }


    /// <summary>
    /// 進捗初期化
    /// </summary>
    public void Clear()
    {
        CurrentStep = 0;
        MaxSteps = 1;
        CurrentStepSub = 0;
        MaxStepsSub = 1;
    }
}

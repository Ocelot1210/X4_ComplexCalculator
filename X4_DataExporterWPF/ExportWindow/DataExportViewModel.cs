using LibX4.FileSystem;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace X4_DataExporterWPF.DataExportWindow;

/// <summary>
/// データ抽出処理用ViewModel
/// </summary>
class DataExportViewModel : BindableBase
{
    #region メンバ
    /// <summary>
    /// モデル
    /// </summary>
    private readonly DataExportModel _Model = new();

    /// <summary>
    /// 出力先ファイルパス
    /// </summary>
    private readonly string _OutFilePath;


    /// <summary>
    /// 現在の入力元フォルダパスから言語一覧を取得できなかった場合 true
    /// </summary>
    private readonly ReactivePropertySlim<bool> _UnableToGetLanguages;


    /// <summary>
    /// 処理状態管理
    /// </summary>
    private readonly BusyNotifier _BusyNotifier = new();


    /// <summary>
    /// 親ウィンドウ(メッセージボックス表示用)
    /// </summary>
    private readonly Window _OwnerWindow;
    #endregion


    #region プロパティ
    /// <summary>
    /// 入力元フォルダパス
    /// </summary>
    public ReactiveProperty<string> InDirPath { get; }


    /// <summary>
    /// 言語一覧
    /// </summary>
    public ReactiveCollection<LangComboboxItem> Languages { get; }


    /// <summary>
    /// 選択された言語
    /// </summary>
    public ReactivePropertySlim<LangComboboxItem?> SelectedLanguage { get; }


    /// <summary>
    /// 読み込みオプション
    /// </summary>
    public ReactivePropertySlim<CatLoadOption> CatLoadOption { get; }


    /// <summary>
    /// 進捗最大
    /// </summary>
    public ReactivePropertySlim<int> MaxSteps { get; } = new(1);


    /// <summary>
    /// 現在の進捗
    /// </summary>
    public ReactivePropertySlim<int> CurrentStep { get; } = new(0);


    /// <summary>
    /// 進捗最大(小項目)
    /// </summary>
    public ReactivePropertySlim<int> MaxStepsSub { get; } = new(1);


    /// <summary>
    /// 現在の進捗(小項目)
    /// </summary>
    public ReactivePropertySlim<int> CurrentStepSub { get; } = new(0);


    /// <summary>
    /// ユーザが操作可能か
    /// </summary>
    public ReadOnlyReactiveProperty<bool> CanOperation { get; }


    /// <summary>
    /// 入力元フォルダ参照
    /// </summary>
    public ReactiveCommand SelectInDirCommand { get; }


    /// <summary>
    /// 抽出実行
    /// </summary>
    public AsyncReactiveCommand ExportCommand { get; }


    /// <summary>
    /// ウィンドウを閉じる
    /// </summary>
    public ReactiveCommand<CancelEventArgs> ClosingCommand { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public DataExportViewModel(string inDirPath, string outFilePath, Window window)
    {
        _OwnerWindow = window;

        CatLoadOption = new ReactivePropertySlim<CatLoadOption>(LibX4.FileSystem.CatLoadOption.All);

        InDirPath = new ReactiveProperty<string>(inDirPath,
            mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe);
        _UnableToGetLanguages = new ReactivePropertySlim<bool>(false);
        InDirPath.SetValidateNotifyError(
            _ => _UnableToGetLanguages.Select(isError => isError ? "Error" : null));
        _OutFilePath = outFilePath;

        Languages = new ReactiveCollection<LangComboboxItem>();
        SelectedLanguage = new ReactivePropertySlim<LangComboboxItem?>();

        CanOperation = _BusyNotifier.Inverse().ToReadOnlyReactiveProperty();

        // 操作可能かつ入力項目に不備がない場合に true にする
        var canExport = new[]{
            CanOperation,
            InDirPath.Select(p => !string.IsNullOrEmpty(p)),
            SelectedLanguage.Select(l => l != null),
        }.CombineLatestValuesAreAllTrue();

        SelectInDirCommand = new ReactiveCommand(CanOperation).WithSubscribe(SelectInDir);
        ExportCommand = new AsyncReactiveCommand(canExport).WithSubscribe(Export);
        ClosingCommand = new ReactiveCommand<CancelEventArgs>().WithSubscribe(Closing);


        // 入力元フォルダパスに値が代入された時、言語一覧を更新する
        InDirPath.Subscribe(async path =>
        {
            await UpdateLangList(path);
        });

        // 抽出オプションが変化した際、言語一覧を更新する
        CatLoadOption.Subscribe(async option =>
        {
            await UpdateLangList(InDirPath.Value);
        });
    }


    /// <summary>
    /// 指定したパスを X4 のインストール先と見なして言語一覧を初期化する
    /// </summary>
    /// <param name="x4InstallDirectory">X4 のインストール先フォルダパス</param>
    /// <returns>言語一覧が更新された場合、<c>true;</c>。それ以外の場合 <c>false;</c></returns>
    private async Task UpdateLangList(string x4InstallDirectory)
    {
        if (_BusyNotifier.IsBusy) return;
        using var _ = _BusyNotifier.ProcessStart();

        var prevLangID = SelectedLanguage.Value?.ID ?? -1;

        _UnableToGetLanguages.Value = false;
        Languages.ClearOnScheduler();

        var (success, languages) = await Task.Run(async () => await _Model.GetLanguages(x4InstallDirectory, _OwnerWindow));
        _UnableToGetLanguages.Value = !success;
        Languages.AddRangeOnScheduler(languages);

        ReactivePropertyScheduler.Default.Schedule(() =>
        {
            SelectedLanguage.Value = Languages.FirstOrDefault(x => x.ID == prevLangID);
        });

        return;
    }


    /// <summary>
    /// 入力元フォルダを選択
    /// </summary>
    private void SelectInDir()
    {
        var dlg = new CommonOpenFileDialog();
        dlg.IsFolderPicker = true;
        dlg.AllowNonFileSystemItems = false;

        if (Directory.Exists(InDirPath.Value))
        {
            dlg.InitialDirectory = InDirPath.Value;
        }
        else
        {
            dlg.InitialDirectory = Path.GetDirectoryName(InDirPath.Value);
        }

        if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
        {
            InDirPath.Value = dlg.FileName;
        }
    }


    /// <summary>
    /// データ抽出実行
    /// </summary>
    private async Task Export()
    {
        using var _ = _BusyNotifier.ProcessStart();

        // 言語が未選択なら何もしない
        if (SelectedLanguage.Value == null)
        {
            return;
        }

        var progress = new Progress<(int currentStep, int maxSteps)>(s =>
        {
            CurrentStep.Value = s.currentStep;
            MaxSteps.Value = s.maxSteps;
        });

        var progressSub = new Progress<(int currentStep, int maxSteps)>(s =>
        {
            CurrentStepSub.Value = s.currentStep;
            MaxStepsSub.Value = s.maxSteps;
        });

        await Task.Run(() => _Model.Export(
            progress,
            progressSub,
            InDirPath.Value,
            _OutFilePath,
            SelectedLanguage.Value,
            _OwnerWindow
        ));
        CurrentStep.Value = 0;
    }


    /// <summary>
    /// ウィンドウを閉じる
    /// </summary>
    /// <param name="e"></param>
    private void Closing(CancelEventArgs e) => e.Cancel = _BusyNotifier.IsBusy;
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LibX4.FileSystem;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace X4_DataExporterWPF.ExportWindows;

/// <summary>
/// データ抽出処理用ViewModel
/// </summary>
sealed partial class DataExportViewModel : ObservableValidator
{
    #region メンバ
    /// <summary>
    /// データ抽出処理用Model
    /// </summary>
    private readonly DataExportModel _model;


    /// <summary>
    /// メッセージ通知用
    /// </summary>
    private readonly IMessenger _messenger;
    #endregion


    #region プロパティ
    /// <summary>
    /// 入力元フォルダパス
    /// </summary>
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(ExportCommand))]
    [InDirPathChecker]
    public partial string InDirPath { get; set; } = "";


    /// <summary>
    /// 言語一覧
    /// </summary>
    public ICollectionView Languages { get; }


    /// <summary>
    /// 言語一覧の取得に失敗したか
    /// </summary>
    [ObservableProperty]
    public partial bool UnableToGetLanguages { get; private set; }


    /// <summary>
    /// 選択された言語
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExportCommand))]
    public partial LangComboboxItem? SelectedLanguage { get; set; }


    /// <summary>
    /// 読み込みオプション
    /// </summary>
    [ObservableProperty]
    public partial CatLoadOption CatLoadOption { get; set; } = CatLoadOption.All;


    /// <summary>
    /// 設定フォルダパス一覧
    /// </summary>
    public ICollectionView ConfigFolderPaths { get; }


    /// <summary>
    /// 選択された設定フォルダ
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExportCommand))]
    public partial string SelectedConfigFolderPath { get; set; } = "";


    /// <summary>
    /// データ抽出進捗
    /// </summary>
    public ExportProgress ExportProgress { get; } = new();


    /// <summary>
    /// ビジー状態か
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExportCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectInDirCommand))]
    public partial bool IsBusy { get; private set; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public DataExportViewModel(IMessenger messenger, string inDirPath, string outFilePath)
    {
        _messenger = messenger;

        _model = new(_messenger, outFilePath);

        ConfigFolderPaths = CollectionViewSource.GetDefaultView(_model.ConfigFolderPaths);
        Languages = CollectionViewSource.GetDefaultView(_model.Languages);

        InDirPath = inDirPath;

        if (_model.ConfigFolderPaths.Any())
        {
            SelectedConfigFolderPath = _model.ConfigFolderPaths.First();
        }
    }


    /// <summary>
    /// <see cref="InDirPath"/> 変更時
    /// </summary>
    partial void OnInDirPathChanged(string value) => _ = UpdateLangList();


    /// <summary>
    /// <see cref="CatLoadOption"/> 変更時
    /// </summary>
    partial void OnCatLoadOptionChanged(CatLoadOption value) => _ = UpdateLangList();


    /// <summary>
    /// 指定したパスを X4 のインストール先と見なして言語一覧を初期化する
    /// </summary>
    private async Task UpdateLangList()
    {
        if (IsBusy) return;

        UnableToGetLanguages = false;
        ClearErrors(nameof(InDirPath));
        
        try
        {
            IsBusy = true;
            
            var prevLangID = SelectedLanguage?.ID ?? -1;

            // 言語を更新
            await Task.Run(async () => await _model.UpdateLanguagesAsync(InDirPath, SelectedConfigFolderPath, CatLoadOption));
            
            // 言語前回値を復元
            SelectedLanguage = _model.Languages.FirstOrDefault(x => x.ID == prevLangID);
        }
        catch (DependencyResolutionException ex)
        {
            UnableToGetLanguages = true;

            _messenger.Send(ex);
        }
        catch (Exception)
        {
            UnableToGetLanguages = true;
        }
        finally
        {
            IsBusy = false;
            ValidateAllProperties();
        }
    }


    /// <summary>
    /// 入力元フォルダを選択
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSelectInDir))]
    private void SelectInDir()
    {
        var dlg = new CommonOpenFileDialog
        {
            IsFolderPicker = true,
            AllowNonFileSystemItems = false
        };

        if (Directory.Exists(InDirPath))
        {
            dlg.InitialDirectory = InDirPath;
        }
        else
        {
            dlg.InitialDirectory = Path.GetDirectoryName(InDirPath);
        }

        if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
        {
            InDirPath = dlg.FileName;
        }
    }


    /// <summary>
    /// 入力元フォルダを選択可能か
    /// </summary>
    private bool CanSelectInDir() => !IsBusy;


    /// <summary>
    /// データ抽出実行
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportAsync()
    {
        if (IsBusy) return;

        // 言語が未選択または設定フォルダ未選択の場合何もしない
        if (SelectedLanguage is null || string.IsNullOrEmpty(SelectedConfigFolderPath))
        {
            return;
        }

        try
        {
            IsBusy = true;
            await Task.Run(() => _model.Export(
                ExportProgress.MainProgress,
                ExportProgress.SubProgress,
                InDirPath,
                SelectedConfigFolderPath,
                CatLoadOption,
                SelectedLanguage
            ));
        }
        finally
        {
            IsBusy = false;
            ExportProgress.Clear();
        }
    }


    /// <summary>
    /// データ抽出実行可能か
    /// </summary>
    private bool CanExport() => !IsBusy && !string.IsNullOrEmpty(InDirPath) && SelectedLanguage is not null && !string.IsNullOrEmpty(SelectedConfigFolderPath);


    /// <summary>
    /// ウィンドウを閉じる
    /// </summary>
    [RelayCommand]
    private void Closing(CancelEventArgs e) => e.Cancel = IsBusy;
}

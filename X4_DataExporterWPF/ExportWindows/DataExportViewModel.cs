using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibX4.FileSystem;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using X4_DataExporterWPF.ExportWindow.DependencyResolutionFailedWindows;

namespace X4_DataExporterWPF.DataExportWindows;

/// <summary>
/// データ抽出処理用ViewModel
/// </summary>
sealed partial class DataExportViewModel : ObservableValidator
{
    #region メンバ
    /// <summary>
    /// 親ウィンドウ(メッセージボックス表示用)
    /// </summary>
    private readonly Window _ownerWindow;


    /// <summary>
    /// データ抽出処理用Model
    /// </summary>
    private readonly DataExportModel _model;
    #endregion


    #region プロパティ
    /// <summary>
    /// 入力元フォルダパス
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanExport))]
    public partial string InDirPath { get; set; } = "";


    /// <summary>
    /// 言語一覧
    /// </summary>
    public ICollectionView Languages { get; }


    /// <summary>
    /// 選択された言語
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanExport))]
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
    [NotifyPropertyChangedFor(nameof(CanExport))]
    public partial string SelectedConfigFolderPath { get; set; } = "";


    /// <summary>
    /// データ抽出進捗
    /// </summary>
    public ExportProgress ExportProgress { get; } = new();


    /// <summary>
    /// ビジー状態か
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanExport))]
    public partial bool IsBusy { get; private set; }


    /// <summary>
    /// エクスポート可能か
    /// </summary>
    public bool CanExport => !IsBusy && !string.IsNullOrEmpty(InDirPath) && SelectedLanguage is not null && !string.IsNullOrEmpty(SelectedConfigFolderPath);
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public DataExportViewModel(string inDirPath, string outFilePath, Window window)
    {
        _model = new(outFilePath);
        _ownerWindow = window;

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

        try
        {
            IsBusy = true;
            //_unableToGetLanguages.Value = false;
            
            var prevLangID = SelectedLanguage?.ID ?? -1;

            // 言語を更新
            await Task.Run(async () => await _model.UpdateLanguagesAsync(InDirPath, SelectedConfigFolderPath, CatLoadOption));
            
            // 言語前回値を復元
            SelectedLanguage = _model.Languages.FirstOrDefault(x => x.ID == prevLangID);
        }
        catch (DependencyResolutionException ex)
        {
            //_unableToGetLanguages.Value = true;

            _ownerWindow.Dispatcher.Invoke(() =>
            {
                var wnd = new DependencyResolutionFailedWindow(ex.UnloadedMods);
                wnd.Owner = _ownerWindow;
                wnd.ShowDialog();
            });
        }
        catch (Exception)
        {
            //_unableToGetLanguages.Value = true;
        }
        finally
        {
            IsBusy = false;
            ValidateProperty(this, nameof(InDirPath));
        }
    }


    /// <summary>
    /// 入力元フォルダを選択
    /// </summary>
    [RelayCommand]
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
    /// データ抽出実行
    /// </summary>
    [RelayCommand]
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
                SelectedLanguage,
                _ownerWindow
            ));
        }
        finally
        {
            IsBusy = false;
            ExportProgress.Clear();
        }
    }


    /// <summary>
    /// ウィンドウを閉じる
    /// </summary>
    [RelayCommand]
    private void Closing(CancelEventArgs e) => e.Cancel = IsBusy;
}

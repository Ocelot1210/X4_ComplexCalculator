using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;

namespace X4_ComplexCalculator.Main.Menu.File.Importers.LoadoutImporters;

/// <summary>
/// モジュール装備インポート画面のViewModel
/// </summary>
/// <param name="localizedMessageBox">メッセージボックス表示用</param>
partial class LoadoutImporterViewModel(ILocalizedMessageBox localizedMessageBox) : ObservableObject
{
    #region メンバ
    /// <summary>
    /// Model
    /// </summary>
    private readonly LoadoutImporterModel _model = new(localizedMessageBox);
    #endregion


    #region プロパティ
    /// <summary>
    /// ダイアログの戻り値
    /// </summary>
    [ObservableProperty]
    public partial bool DialogResult { get; private set; }


    /// <summary>
    /// ダイアログを閉じるか
    /// </summary>
    [ObservableProperty]
    public partial bool CloseDialogProperty { get; set; }


    /// <summary>
    /// チェック/全チェック変更時
    /// </summary>
    public bool? IsCheckedAll
    {
        get
        {
            var @checked = Loadouts.Count(x => x.IsChecked);

            return (@checked == 0) ? false :
                   (@checked == Loadouts.Count) ? (bool?)true : null;
        }
        set
        {
            foreach (var station in Loadouts)
            {
                station.IsChecked = value ?? false;
            }
        }
    }


    /// <summary>
    /// セーブデータファイルパス
    /// </summary>
    public string LoadoutsFilePath => _model.LoadoutsFilePath;


    /// <summary>
    /// 計画一覧
    /// </summary>
    public ObservableCollection<LoadoutItem> Loadouts => _model.Loadouts;
    #endregion


    /// <summary>
    /// 建造計画ファイル選択
    /// </summary>
    [RelayCommand]
    private void SelectSaveDataFile() => _model.SelectSaveDataFile();


    /// <summary>
    /// インポート実行
    /// </summary>
    [RelayCommand]
    private void Import() => _model.Import();


    /// <summary>
    /// 閉じるボタンクリック時
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        DialogResult = true;
        CloseDialogProperty = true;
    }
}

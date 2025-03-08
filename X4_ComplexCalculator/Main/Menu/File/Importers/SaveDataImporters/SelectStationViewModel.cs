using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;

namespace X4_ComplexCalculator.Main.Menu.File.Importers.SaveDataImporters;

partial class SelectStationViewModel : ObservableObject
{
    #region メンバ
    /// <summary>
    /// Model
    /// </summary>
    private readonly SelectStationModel _model;
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
    public partial bool CloseDialogProperty { get; private set; }


    /// <summary>
    /// 全選択されているか
    /// </summary>
    public bool? IsCheckedAll
    {
        get
        {
            var @checked = Stations.Where(x => x.IsChecked).Count();

            return (@checked == 0) ? (bool?)false :
                   (@checked == Stations.Count) ? (bool?)true : null;
        }
        set
        {
            foreach (var station in Stations)
            {
                station.IsChecked = value ?? false;
            }
        }
    }


    /// <summary>
    /// セーブデータファイルパス
    /// </summary>
    public string SaveDataFilePath => _model.SaveDataFilePath;


    /// <summary>
    /// 計画一覧
    /// </summary>
    public ObservableCollection<SaveDataStationItem> Stations => _model.Stations;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="stationItems">選択されたステーション格納先</param>
    /// <param name="messageBox">メッセージボックス表示用</param>
    public SelectStationViewModel(List<SaveDataStationItem> stationItems, ILocalizedMessageBox messageBox)
    {
        _model = new SelectStationModel(stationItems, messageBox);
    }


    /// <summary>
    /// 建造計画ファイル選択
    /// </summary>
    [RelayCommand]
    private void SelectSaveDataFile() => _model.SelectSaveDataFile();


    /// <summary>
    /// OKボタンクリック時
    /// </summary>
    [RelayCommand]
    private void OnOk()
    {
        _model.OnOk();
        DialogResult = true;
        CloseDialogProperty = true;
    }


    /// <summary>
    /// キャンセルボタンクリック時
    /// </summary>
    [RelayCommand]
    private void OnCancel()
    {
        DialogResult = false;
        CloseDialogProperty = true;
    }
}

using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace X4_ComplexCalculator.Main.Menu.File.Import.SaveDataImport;

class SelectStationViewModel : BindableBase
{
    #region メンバ
    /// <summary>
    /// Model
    /// </summary>
    private readonly SelectStationModel _model;


    /// <summary>
    /// ダイアログの戻り値
    /// </summary>
    private bool _dialogResult;


    /// <summary>
    /// ダイアログを閉じるか
    /// </summary>
    private bool _closeDialogProperty;


    /// <summary>
    /// 選択された計画一覧
    /// </summary>
    private readonly List<SaveDataStationItem> _selectedStationItems;
    #endregion


    #region プロパティ

    /// <summary>
    /// ダイアログの戻り値
    /// </summary>
    public bool DialogResult
    {
        get => _dialogResult;
        set => SetProperty(ref _dialogResult, value);
    }

    /// <summary>
    /// ダイアログを閉じるか
    /// </summary>
    public bool CloseDialogProperty
    {
        get => _closeDialogProperty;
        set => SetProperty(ref _closeDialogProperty, value);
    }

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


    /// <summary>
    /// 建造計画ファイル選択
    /// </summary>
    public ICommand SelectSaveDataFileCommand { get; }


    /// <summary>
    /// OKボタンクリック時の処理
    /// </summary>
    public ICommand OkButtonClickedCommand { get; }


    /// <summary>
    /// キャンセルボタンクリック時の処理
    /// </summary>
    public ICommand CancelButtonClickedCommand { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="stationItems"></param>
    public SelectStationViewModel(List<SaveDataStationItem> stationItems)
    {
        _model = new SelectStationModel();
        _selectedStationItems = stationItems;
        OkButtonClickedCommand      = new DelegateCommand(OkButtonClicked);
        CancelButtonClickedCommand  = new DelegateCommand(CancelButtonClicked);
        SelectSaveDataFileCommand   = new DelegateCommand(_model.SelectSaveDataFile);
    }


    /// <summary>
    /// OKボタンクリック時
    /// </summary>
    private void OkButtonClicked()
    {
        _selectedStationItems.AddRange(Stations.Where(x => x.IsChecked));
        DialogResult = true;
        CloseDialogProperty = true;
    }


    /// <summary>
    /// キャンセルボタンクリック時
    /// </summary>
    private void CancelButtonClicked()
    {
        _selectedStationItems.Clear();
        DialogResult = false;
        CloseDialogProperty = true;
    }
}

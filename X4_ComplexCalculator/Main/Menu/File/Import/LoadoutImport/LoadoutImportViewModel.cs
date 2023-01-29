using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace X4_ComplexCalculator.Main.Menu.File.Import.LoadoutImport;

/// <summary>
/// モジュール装備インポート画面のViewModel
/// </summary>
class LoadoutImportViewModel : BindableBase
{
    #region メンバ
    /// <summary>
    /// Model
    /// </summary>
    private readonly LoadoutImportModel _model;


    /// <summary>
    /// ダイアログの戻り値
    /// </summary>
    private bool _dialogResult;


    /// <summary>
    /// ダイアログを閉じるか
    /// </summary>
    private bool _closeDialogProperty;
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


    /// <summary>
    /// 建造計画ファイル選択
    /// </summary>
    public ICommand SelectSaveDataFileCommand { get; }


    /// <summary>
    /// インポートボタンクリック時の処理
    /// </summary>
    public ICommand ImportButtonClickedCommand { get; }


    /// <summary>
    /// 閉じるボタンクリック時の処理
    /// </summary>
    public ICommand CloseButtonClickedCommand { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public LoadoutImportViewModel()
    {
        _model = new LoadoutImportModel();
        ImportButtonClickedCommand = new DelegateCommand(_model.Import);
        CloseButtonClickedCommand  = new DelegateCommand(CloseButtonClicked);
        SelectSaveDataFileCommand  = new DelegateCommand(_model.SelectSaveDataFile);
    }


    /// <summary>
    /// 閉じるボタンクリック時
    /// </summary>
    private void CloseButtonClicked()
    {
        DialogResult = true;
        CloseDialogProperty = true;
    }
}

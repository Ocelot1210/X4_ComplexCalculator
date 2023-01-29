using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace X4_ComplexCalculator.Main.Menu.File.Import.StationPlanImport;

class SelectPlanViewModel : BindableBase
{
    #region メンバ
    /// <summary>
    /// Model
    /// </summary>
    private readonly SelectPlanModel _model;


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
    private readonly List<StationPlanItem> _selectedPlanItems;
    #endregion


    #region プロパティ
    /// <summary>
    /// 計画一覧
    /// </summary>
    public ObservableCollection<StationPlanItem> Planes => _model.Planes;


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
    /// 計画ファイルパス
    /// </summary>
    public string PlanFilePath => _model.PlanFilePath;


    /// <summary>
    /// 建造計画ファイル選択
    /// </summary>
    public ICommand SelectPlanFileCommand { get; }


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
    /// <param name="planItems">選択計画一覧</param>
    public SelectPlanViewModel(List<StationPlanItem> planItems)
    {
        _model = new SelectPlanModel();
        _model.PropertyChanged += Model_PropertyChanged;
        _selectedPlanItems = planItems;

        OkButtonClickedCommand     = new DelegateCommand(OkButtonClicked);
        CancelButtonClickedCommand = new DelegateCommand(CancelButtonClicked);
        SelectPlanFileCommand      = new DelegateCommand(_model.SelectPlanFile);
    }


    /// <summary>
    /// Modelのプロパティ変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectPlanModel.PlanFilePath):
                RaisePropertyChanged(nameof(PlanFilePath));
                break;

            default:
                break;
        }
    }


    /// <summary>
    /// OKボタンクリック時
    /// </summary>
    private void OkButtonClicked()
    {
        _selectedPlanItems.AddRange(Planes.Where(x => x.IsChecked));
        DialogResult = true;
        CloseDialogProperty = true;
    }


    /// <summary>
    /// キャンセルボタンクリック時
    /// </summary>
    private void CancelButtonClicked()
    {
        DialogResult = false;
        CloseDialogProperty = true;
    }
}

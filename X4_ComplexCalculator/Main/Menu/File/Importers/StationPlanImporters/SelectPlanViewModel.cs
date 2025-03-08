using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;

namespace X4_ComplexCalculator.Main.Menu.File.Importers.StationPlanImporters;

sealed partial class SelectPlanViewModel : ObservableObject
{
    #region メンバ
    /// <summary>
    /// Model
    /// </summary>
    private readonly SelectPlanModel _model;


    /// <summary>
    /// 選択された計画一覧
    /// </summary>
    private readonly List<StationPlanItem> _selectedPlanItems;
    #endregion


    #region プロパティ
    /// <summary>
    /// 計画一覧
    /// </summary>
    public ObservableCollection<StationPlanItem> Planes => _model.Plans;


    /// <summary>
    /// ダイアログの戻り値
    /// </summary>
    [ObservableProperty]
    public partial bool DialogResult { get; set; }


    /// <summary>
    /// ダイアログを閉じるか
    /// </summary>
    [ObservableProperty]
    public partial bool CloseDialogProperty { get; set; }


    /// <summary>
    /// 計画ファイルパス
    /// </summary>
    public string PlanFilePath => _model.PlanFilePath;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="planItems">選択計画一覧</param>
    /// <param name="messageBox">メッセージボックス表示用</param>
    public SelectPlanViewModel(List<StationPlanItem> planItems, ILocalizedMessageBox messageBox)
    {
        _model = new SelectPlanModel(messageBox);
        _model.PropertyChanged += Model_PropertyChanged;
        _selectedPlanItems = planItems;
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
                OnPropertyChanged(nameof(PlanFilePath));
                break;

            default:
                break;
        }
    }


    /// <summary>
    /// OKボタンクリック時
    /// </summary>
    [RelayCommand]
    private void OnOkButtonClicked()
    {
        _selectedPlanItems.AddRange(Planes.Where(x => x.IsChecked));
        DialogResult = true;
        CloseDialogProperty = true;
    }


    /// <summary>
    /// キャンセルボタンクリック時
    /// </summary>
    [RelayCommand]
    private void OnCancelButtonClicked()
    {
        DialogResult = false;
        CloseDialogProperty = true;
    }


    /// <summary>
    /// 建造計画ファイルを選択
    /// </summary>
    [RelayCommand]
    private void OnSelectPlanFile() => _model.SelectPlanFile();
}

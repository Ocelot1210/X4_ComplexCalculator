using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StoragesGrid;

/// <summary>
/// 保管庫一覧表示用DataGridViewのViewModel
/// </summary>
public class StoragesGridViewModel : BindableBase, IDisposable
{
    #region メンバ
    /// <summary>
    /// 保管庫一覧表示用DataGridViewのModel
    /// </summary>
    private readonly StoragesGridModel _Model;
    #endregion


    #region プロパティ
    /// <summary>
    /// ストレージ一覧
    /// </summary>
    public ObservableCollection<StoragesGridItem> Storages => _Model.Storages;


    /// <summary>
    /// 選択されたアイテムの展開/折りたたみ状態を設定する
    /// </summary>
    public ICommand SetSelectedExpandedCommand { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="stationData">計算機で使用するステーション情報</param>
    public StoragesGridViewModel(IStationData stationData)
    {
        _Model = new StoragesGridModel(stationData.ModulesInfo, stationData.StoragesInfo);
        SetSelectedExpandedCommand = new DelegateCommand<bool?>(SetSelectedExpanded);
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _Model.Dispose();
    }


    /// <summary>
    /// 選択されたアイテムの展開/折りたたみ状態を設定する
    /// </summary>
    /// <param name="param"></param>
    private void SetSelectedExpanded(bool? param)
    {
        foreach (var item in Storages.Where(x => x.IsSelected))
        {
            item.IsExpanded = param == true;
        }
    }
}

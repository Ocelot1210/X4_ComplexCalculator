using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StoragesGrid;

/// <summary>
/// 保管庫一覧表示用DataGridViewのViewModel
/// </summary>
public sealed partial class StoragesGridViewModel : ObservableObject, IDisposable
{
    #region メンバ
    /// <summary>
    /// 保管庫一覧表示用DataGridViewのModel
    /// </summary>
    private readonly StoragesGridModel _model;
    #endregion


    #region プロパティ
    /// <summary>
    /// ストレージ一覧
    /// </summary>
    public ObservableCollection<StoragesGridItem> Storages => _model.Storages;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="stationData">計算機で使用するステーション情報</param>
    public StoragesGridViewModel(IMessenger messenger, StationData stationData)
    {
        _model = new StoragesGridModel(messenger, stationData.ModulesInfo, stationData.StoragesInfo);
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _model.Dispose();
    }


    /// <summary>
    /// 選択されたアイテムの展開/折りたたみ状態を設定する
    /// </summary>
    /// <param name="param">設定値</param>
    [RelayCommand]
    private void SetSelectedExpanded(bool? param) => _model.SetExpanded(param == true);
}

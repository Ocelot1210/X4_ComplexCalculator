using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Data;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment.EquipmentList;

/// <summary>
/// 装備一覧用ViewModel
/// </summary>
class EquipmentListViewModel : BindableBase, IDisposable
{
    #region メンバ
    /// <summary>
    /// 装備一覧用Model
    /// </summary>
    private readonly EquipmentListModel _Model;


    /// <summary>
    /// ゴミ箱
    /// </summary>
    private readonly CompositeDisposable _Disposables = new();


    /// <summary>
    /// 派閥一覧
    /// </summary>
    private readonly ObservablePropertyChangedCollection<FactionsListItem> _Factions;
    #endregion


    #region プロパティ
    /// <summary>
    /// タイトル文字列
    /// </summary>
    public string Title => _Model.Title;


    /// <summary>
    /// 装備一覧表示用
    /// </summary>
    public ListCollectionView EquipmentsView { get; }


    /// <summary>
    /// 装備中の装備
    /// </summary>
    public ObservableCollection<EquipmentListItem> Equipped => _Model.Equipped;


    /// <summary>
    /// 装備中の装備
    /// </summary>
    public ListCollectionView EquippedView { get; }



    /// <summary>
    /// 装備可能な個数
    /// </summary>
    public ReadOnlyReactiveProperty<int> MaxAmount { get; }



    /// <summary>
    /// 現在装備中の個数
    /// </summary>
    public ReadOnlyReactiveProperty<int> EquippedCount { get; }


    /// <summary>
    /// 装備の検索文字列
    /// </summary>
    public ReactiveProperty<string> SearchEquipmentName { get; } = new("");


    /// <summary>
    /// 追加ボタンクリック時のコマンド
    /// </summary>
    public ReactiveCommand AddButtonClickedCommand { get; }


    /// <summary>
    /// 削除ボタンクリック時のコマンド
    /// </summary>
    public ReactiveCommand RemoveButtonClickedCommand { get; }



    /// <summary>
    /// 選択中の装備サイズ
    /// </summary>
    public ReactiveProperty<IX4Size> SelectedSize { get; }


    /// <summary>
    /// 選択中のプリセット
    /// </summary>
    public ReactiveProperty<PresetComboboxItem?> SelectedPreset { get; }


    /// <summary>
    /// 未保存か
    /// </summary>
    public ReactiveProperty<bool> Unsaved { get; }
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="model">装備一覧用Model</param>
    /// <param name="factions">派閥一覧</param>
    public EquipmentListViewModel(
        EquipmentListModel model,
        ObservablePropertyChangedCollection<FactionsListItem> factions
    )
    {
        _Model = model;

        _Factions = factions;
        _Factions.CollectionPropertyChanged += Factions_CollectionPropertyChanged;

        // 選択中の装備サイズ
        SelectedSize = _Model.SelectedSize
            .ToReactivePropertyAsSynchronized(x => x.Value)
            .AddTo(_Disposables);

        // 装備可能個数
        MaxAmount = _Model
            .ObserveProperty(x => x.MaxAmount)
            .ToReadOnlyReactiveProperty()
            .AddTo(_Disposables);

        // 装備中の個数
        EquippedCount = _Model
            .ObserveProperty(x => x.EquippedCount)
            .ToReadOnlyReactiveProperty()
            .AddTo(_Disposables);

        // 装備追加ボタンクリック
        AddButtonClickedCommand = EquippedCount
            .CombineLatest(MaxAmount, (eqp, max) => eqp < max)
            .ToReactiveCommand()
            .AddTo(_Disposables);
        AddButtonClickedCommand
            .Subscribe(AddButtonClicked)
            .AddTo(_Disposables);

        // 装備削除ボタンクリック
        RemoveButtonClickedCommand = EquippedCount
            .Select(x => 0 < x)
            .ToReactiveCommand()
            .AddTo(_Disposables);
        RemoveButtonClickedCommand
            .Subscribe(RemoveButtonClicked)
            .AddTo(_Disposables);

        // 未保存か
        Unsaved = _Model.Unsaved
            .ToReactiveProperty()
            .AddTo(_Disposables);

        // 選択中のプリセット
        SelectedPreset = _Model.SelectedPreset
            .ToReactivePropertyAsSynchronized(x => x.Value)
            .AddTo(_Disposables);


        EquipmentsView = (ListCollectionView)CollectionViewSource.GetDefaultView(model.Equippable);
        EquipmentsView.Filter = EquipmentsFilter;

        EquippedView = (ListCollectionView)CollectionViewSource.GetDefaultView(model.Equipped);
        EquippedView.Filter = EquippedFilter;

        // 装備一覧更新用
        SearchEquipmentName
            .Subscribe(x => EquipmentsView.Refresh())
            .AddTo(_Disposables);
        SelectedSize
            .Subscribe(x => { EquipmentsView.Refresh(); EquippedView.Refresh(); })
            .AddTo(_Disposables);
    }




    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _Factions.CollectionPropertyChanged -= Factions_CollectionPropertyChanged;
        _Disposables.Dispose();
    }


    /// <summary>
    /// 派閥のチェック変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Factions_CollectionPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        EquipmentsView.Refresh();
    }


    /// <summary>
    /// 追加ボタンクリック時
    /// </summary>
    void AddButtonClicked()
    {
        if (_Model.AddSelectedEquipments())
        {
            // 装備が追加された場合
            Unsaved.Value = true;
        }
    }


    /// <summary>
    /// 削除ボタンクリック時
    /// </summary>
    void RemoveButtonClicked()
    {
        if (_Model.RemoveSelectedEquipments())
        {
            // 装備が削除された場合
            Unsaved.Value = true;
        }
    }



    /// <summary>
    /// フィルタイベント(装備済み用)
    /// </summary>
    /// <param name="obj">評価対象</param>
    /// <returns>評価対象を表示するか</returns>
    private bool EquippedFilter(object obj)
    {
        if (obj is EquipmentListItem item)
        {
            item.IsSelected = false;

            // サイズ違いなら表示しない
            if (!item.Equipment.EquipmentTags.Contains(SelectedSize.Value.SizeID))
            {
                return false;
            }

            return true;
        }

        return false;
    }


    /// <summary>
    /// フィルタイベント(装備一覧用)
    /// </summary>
    /// <param name="obj">評価対象</param>
    /// <returns>評価対象を表示するか</returns>
    private bool EquipmentsFilter(object obj)
    {
        if (obj is EquipmentListItem item)
        {
            item.IsSelected = false;

            // サイズ違いなら表示しない
            if (!item.Equipment.EquipmentTags.Contains(SelectedSize.Value.SizeID))
            {
                return false;
            }

            // 所有派閥でなければ表示しない
            if (!item.Equipment.Owners.Intersect(_Factions.Where(x => x.IsChecked).Select(x => x.Faction)).Any())
            {
                return false;
            }

            // フィルタが空なら表示する
            if (SearchEquipmentName.Value == "")
            {
                return true;
            }

            return 0 <= item.Equipment.Name.IndexOf(SearchEquipmentName.Value, StringComparison.InvariantCultureIgnoreCase);
        }

        return false;
    }
}

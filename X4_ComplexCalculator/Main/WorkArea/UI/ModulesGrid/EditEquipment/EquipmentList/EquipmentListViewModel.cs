﻿using Prism.Mvvm;
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
    private readonly EquipmentListModel _model;


    /// <summary>
    /// ゴミ箱
    /// </summary>
    private readonly CompositeDisposable _disposables = new();


    /// <summary>
    /// 派閥一覧
    /// </summary>
    private readonly ObservablePropertyChangedCollection<FactionsListItem> _factions;
    #endregion


    #region プロパティ
    /// <summary>
    /// タイトル文字列
    /// </summary>
    public string Title => _model.Title;


    /// <summary>
    /// 装備一覧表示用
    /// </summary>
    public ListCollectionView EquipmentsView { get; }


    /// <summary>
    /// 装備中の装備
    /// </summary>
    public ObservableCollection<EquipmentListItem> Equipped => _model.Equipped;


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
        _model = model;

        _factions = factions;
        _factions.CollectionPropertyChanged += Factions_CollectionPropertyChanged;

        // 選択中の装備サイズ
        SelectedSize = _model.SelectedSize
            .ToReactivePropertyAsSynchronized(x => x.Value)
            .AddTo(_disposables);

        // 装備可能個数
        MaxAmount = _model
            .ObserveProperty(x => x.MaxAmount)
            .ToReadOnlyReactiveProperty()
            .AddTo(_disposables);

        // 装備中の個数
        EquippedCount = _model
            .ObserveProperty(x => x.EquippedCount)
            .ToReadOnlyReactiveProperty()
            .AddTo(_disposables);

        // 装備追加ボタンクリック
        AddButtonClickedCommand = EquippedCount
            .CombineLatest(MaxAmount, (eqp, max) => eqp < max)
            .ToReactiveCommand()
            .AddTo(_disposables);
        AddButtonClickedCommand
            .Subscribe(AddButtonClicked)
            .AddTo(_disposables);

        // 装備削除ボタンクリック
        RemoveButtonClickedCommand = EquippedCount
            .Select(x => 0 < x)
            .ToReactiveCommand()
            .AddTo(_disposables);
        RemoveButtonClickedCommand
            .Subscribe(RemoveButtonClicked)
            .AddTo(_disposables);

        // 未保存か
        Unsaved = _model.Unsaved
            .ToReactiveProperty()
            .AddTo(_disposables);

        // 選択中のプリセット
        SelectedPreset = _model.SelectedPreset
            .ToReactivePropertyAsSynchronized(x => x.Value)
            .AddTo(_disposables);


        EquipmentsView = (ListCollectionView)CollectionViewSource.GetDefaultView(model.Equippable);
        EquipmentsView.Filter = EquipmentsFilter;

        EquippedView = (ListCollectionView)CollectionViewSource.GetDefaultView(model.Equipped);
        EquippedView.Filter = EquippedFilter;

        // 装備一覧更新用
        SearchEquipmentName
            .Subscribe(x => EquipmentsView.Refresh())
            .AddTo(_disposables);
        SelectedSize
            .Subscribe(x => { EquipmentsView.Refresh(); EquippedView.Refresh(); })
            .AddTo(_disposables);
    }




    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _factions.CollectionPropertyChanged -= Factions_CollectionPropertyChanged;
        _disposables.Dispose();
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
        if (_model.AddSelectedEquipments())
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
        if (_model.RemoveSelectedEquipments())
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
            if (!item.Equipment.Owners.Intersect(_factions.Where(x => x.IsChecked).Select(x => x.Faction)).Any())
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

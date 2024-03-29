﻿using Collections.Pooled;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Dialog.MessageBoxes;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Products;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;

/// <summary>
/// 製品一覧用DataGridViewのModel
/// </summary>
class ProductsGridModel : BindableBase, IDisposable
{
    #region メンバ
    /// <summary>
    /// モジュール一覧
    /// </summary>
    private readonly IModulesInfo _modules;


    /// <summary>
    /// ステーションの設定
    /// </summary>
    private readonly IStationSettings _settings;


    /// <summary>
    /// 製品情報
    /// </summary>
    private readonly IProductsInfo _products;


    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _messageBox;


    /// <summary>
    /// 生産性
    /// </summary>
    private double _efficiency;


    /// <summary>
    /// 製品計算機
    /// </summary>
    private readonly ProductCalculator _productCalculator = ProductCalculator.Instance;


    /// <summary>
    /// 前回値オプション保存用
    /// </summary>
    private readonly Dictionary<string, ProductsGridItem> _optionsBakDict = new();
    #endregion


    #region プロパティ
    /// <summary>
    /// 製品一覧
    /// </summary>
    public ObservableCollection<ProductsGridItem> Products => _products.Products;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="modules">モジュール一覧</param>
    /// <param name="settings">ステーションの設定</param>
    /// <param name="messageBox">メッセージボックス表示用</param>
    public ProductsGridModel(IModulesInfo modules, IProductsInfo products, IStationSettings settings, ILocalizedMessageBox messageBox)
    {
        _modules = modules;
        _products = products;
        _messageBox = messageBox;

        _modules.Modules.CollectionChanged += OnModulesChanged;
        _modules.Modules.CollectionPropertyChanged += OnModulePropertyChanged;

        _modules = modules;
        _settings = settings;
        _settings.PropertyChanged += Settings_PropertyChanged;
        _settings.Workforce.PropertyChanged += Workforce_PropertyChanged;
    }



    /// <summary>
    /// 労働者情報に変更があった場合
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Workforce_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            // 現在労働者数と必要労働者数の割合
            case nameof(WorkforceManager.Proportion):
                UpdateWorkerEfficiency();
                break;

            default:
                break;
        }
    }


    /// <summary>
    /// 設定に変更があった場合
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            // 日光
            case nameof(IStationSettings.Sunlight):
                foreach (var prod in Products)
                {
                    prod.SetEfficiency("sunlight", _settings.Sunlight);
                }
                break;

            default:
                break;
        }
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        Products.Clear();
        _modules.Modules.CollectionChanged -= OnModulesChanged;
        _modules.Modules.CollectionPropertyChanged -= OnModulePropertyChanged;
        _settings.PropertyChanged -= Settings_PropertyChanged;
        _settings.Workforce.PropertyChanged -= Workforce_PropertyChanged;
    }


    /// <summary>
    /// モジュール自動追加
    /// </summary>
    public void AutoAddModule()
    {
        var result = _messageBox.YesNo("Lang:Modules_Button_AutoAdd_ConfirmMessage", "Lang:Common_MessageBoxTitle_Confirmation", LocalizedMessageBoxResult.No);
        if (result != LocalizedMessageBoxResult.Yes)
        {
            return;
        }

        var addedRecords = 0L;              // 追加レコード数
        var addedModules = 0L;              // 追加モジュール数

        // モジュール自動追加で追加されたモジュール一覧
        using var autoAddedModules = new PooledDictionary<string, ModulesGridItem>();


        while (true)
        {
            // 追加モジュールIDとモジュール数のペア一覧
            var addModules = _productCalculator.CalcNeedModules(Products, _settings);

            // 追加モジュールが無ければ(不足が無くなれば)終了
            if (!addModules.Any())
            {
                break;
            }

            using var addTarget = new PooledList<ModulesGridItem>();      // 実際に追加するモジュール一覧

            foreach (var (module, count) in addModules)
            {
                // モジュール自動追加作業用に実際に追加するモジュールが存在するか？
                if (autoAddedModules.ContainsKey(module.ID))
                {
                    // モジュール自動追加作業用に実際に追加するモジュールが存在する場合、
                    // モジュール数を増やしてレコードがなるべく増えないようにする
                    autoAddedModules[module.ID].ModuleCount += count;
                }
                else
                {
                    // モジュール自動追加作業用に実際に追加するモジュールが存在しない場合、
                    // 実際に追加するモジュールと見なす
                    var mgi = new ModulesGridItem(module, null, count) { EditStatus = EditStatus.Edited };
                    addTarget.Add(mgi);
                    autoAddedModules.Add(module.ID, mgi);

                    // 追加レコード数更新
                    addedRecords++;
                }

                // 追加モジュール数更新
                addedModules += count;
            }

            // モジュール一覧に追加対象モジュールを追加
            _modules.Modules.AddRange(addTarget);
        }


        if (addedRecords == 0)
        {
            _messageBox.Ok("Lang:Modules_Button_AutoAdd_NoAddedModulesMessage", "Lang:Common_MessageBoxTitle_Confirmation");
        }
        else
        {
            _messageBox.Ok("Lang:Modules_Button_AutoAdd_AddedModulesMessage", "Lang:Common_MessageBoxTitle_Confirmation", addedRecords, addedModules);
        }
    }


    /// <summary>
    /// モジュールのプロパティが変更された場合
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private void OnModulePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // モジュール数変更時以外は処理しない
        if (e.PropertyName != nameof(ModulesGridItem.ModuleCount))
        {
            return;
        }

        if (sender is not ModulesGridItem module)
        {
            return;
        }

        // 製品/消費ウェアがあるモジュールなら再計算する
        if (module.Module.Resources.Any() || module.Module.Products.Any())
        {
            if (e is not PropertyChangedExtendedEventArgs<long> ev)
            {
                return;
            }

            OnModuleCountChanged(module, ev.OldValue);
        }
    }


    /// <summary>
    /// 製品更新
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnModulesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            OnModuleAdded(e.NewItems.Cast<ModulesGridItem>());
        }

        if (e.OldItems is not null)
        {
            OnModuleRemoved(e.OldItems.Cast<ModulesGridItem>());
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            // 前回値保存
            foreach (var prod in Products)
            {
                if (!_optionsBakDict.TryAdd(prod.Ware.ID, prod))
                {
                    _optionsBakDict[prod.Ware.ID] = prod;
                }
            }

            Products.Clear();

            if (_modules.Modules.Any())
            {
                var products = AggregateProduct(_modules.Modules);

                // 可能なら前回値復元して製品一覧に追加
                var addItems = products.Select
                (
                    x =>
                    {
                        if (_optionsBakDict.TryGetValue(x.Key.ID, out var oldProd))
                        {
                            return new ProductsGridItem(x.Key, x.Value, new TradeOption(oldProd.NoBuy, oldProd.NoSell), oldProd.UnitPrice) { EditStatus = oldProd.EditStatus };
                        }

                        return new ProductsGridItem(x.Key, x.Value, new TradeOption());
                    }
                );

                _products.Products.AddRange(addItems);
                _optionsBakDict.Clear();
            }
        }
    }


    /// <summary>
    /// 生産性を更新
    /// </summary>
    private void UpdateWorkerEfficiency()
    {
        // 労働者による生産性(倍率)
        double efficiency = _settings.Workforce.Proportion;

        if (1.0 < efficiency)
        {
            efficiency = 1.0;
        }

        // 労働による生産性が変化しない場合、何もしない
        if (efficiency == _efficiency)
        {
            return;
        }

        foreach (var prod in Products)
        {
            prod.SetEfficiency("work", efficiency);
        }

        _efficiency = efficiency;
    }


    /// <summary>
    /// モジュール数変更時
    /// </summary>
    /// <param name="module">変更があったモジュール</param>
    /// <param name="prevModuleCount">変更前モジュール数</param>
    private void OnModuleCountChanged(ModulesGridItem module, long prevModuleCount)
    {
        ModulesGridItem[] modules = { module };

        var prodDict = AggregateProduct(modules);

        foreach (var item in prodDict)
        {
            // 変更対象のウェアを検索
            Products.FirstOrDefault(x => x.Ware.Equals(item.Key))?.SetDetails(item.Value, prevModuleCount);
        }
    }


    /// <summary>
    /// モジュールが追加された場合
    /// </summary>
    /// <param name="addedModules">追加されたモジュール</param>
    private void OnModuleAdded(IEnumerable<ModulesGridItem> addedModules)
    {
        // 生産品集計用ディクショナリ
        var prodDict = AggregateProduct(addedModules);

        using var addItems = new PooledList<ProductsGridItem>();
        foreach (var item in prodDict)
        {
            // すでにウェアが存在するか検索
            var prod = Products.FirstOrDefault(x => x.Ware.Equals(item.Key));
            if (prod is not null)
            {
                // ウェアが一覧にある場合
                prod.AddDetails(item.Value);
            }
            else
            {
                // ウェアが一覧に無い場合
                addItems.Add(new ProductsGridItem(item.Key, item.Value, new TradeOption()) { EditStatus = EditStatus.Edited });
            }
        }

        _products.Products.AddRange(addItems);
    }


    /// <summary>
    /// モジュールが削除された場合
    /// </summary>
    /// <param name="removedModules">追加されたモジュール</param>
    private void OnModuleRemoved(IEnumerable<ModulesGridItem> removedModules)
    {
        // 生産品集計用ディクショナリ
        var prodDict = AggregateProduct(removedModules);

        foreach (var item in prodDict)
        {
            // 一致するウェアの詳細情報を削除
            Products.FirstOrDefault(x => x.Ware.Equals(item.Key))?.RemoveDetails(item.Value);
        }

        _products.Products.RemoveAll(x => !x.Details.Any());
    }


    /// <summary>
    /// 製品情報を集計
    /// </summary>
    /// <param name="targetModules">集計対象モジュール</param>
    /// <returns>集計結果</returns>
    private IReadOnlyDictionary<IWare, IReadOnlyList<IProductDetailsListItem>> AggregateProduct(IEnumerable<ModulesGridItem> targetModules)
    {
        return targetModules
            .Where(x => x.Module.Resources.Any() || x.Module.Products.Any())
            .GroupBy(x => x.Module)
            .Select(x => (Module: x.Key, Count: x.Sum(y => y.ModuleCount)))
            .SelectMany(x => _productCalculator.Calc(x.Module, x.Count))
            .GroupBy(x => x.WareID)
            .ToDictionary(
                x => X4Database.Instance.Ware.Get(x.Key),
                x => x.GroupBy(x => x.Module)
                    .SelectMany(
                        x => x.Select(
                            y => y.Efficiency is not null ?
                                (IProductDetailsListItem)new ProductDetailsListItem(y.WareID, y.Module, x.Sum(z => z.ModuleCount), y.Efficiency, x.Sum(z => z.WareAmount), _settings) :
                                new ProductDetailsListItemConsumption(y.WareID, y.Module, x.Sum(z => z.ModuleCount), x.Sum(z => z.WareAmount)))
                        ).ToArray() as IReadOnlyList<IProductDetailsListItem>
            );

    }
}

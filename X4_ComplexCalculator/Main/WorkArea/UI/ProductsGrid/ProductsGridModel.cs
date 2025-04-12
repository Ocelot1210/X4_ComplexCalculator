using Collections.Pooled;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.EditStatus;
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
sealed partial class ProductsGridModel : ObservableRecipientEx, IDisposable
{
    #region メンバ
    /// <summary>
    /// モジュール一覧
    /// </summary>
    private readonly ModulesInfo _modules;


    /// <summary>
    /// ステーションの設定
    /// </summary>
    private readonly StationSettingInfo _settings;


    /// <summary>
    /// 製品情報
    /// </summary>
    private readonly ProductsInfo _products;


    /// <summary>
    /// 生産性
    /// </summary>
    private double _efficiency;


    /// <summary>
    /// 製品計算機
    /// </summary>
    private readonly ProductCalculator _productCalculator = ProductCalculator.Instance;
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
    public ProductsGridModel(IMessenger messenger, ModulesInfo modules, ProductsInfo products, StationSettingInfo settings) : base(messenger, true)
    {
        _modules = modules;
        _products = products;

        _modules.Modules.CollectionChanged += OnModulesChanged;

        _modules = modules;
        _settings = settings;

        Messenger.RegisterPropertyChangedMessage(this, static (ModulesGridItem x)    => x.ModuleCount, static (r, m) => r.OnModuleCountChanged(m));
        Messenger.RegisterPropertyChangedMessage(this, static (StationSettingInfo x) => x.Sunlight,    static (r, m) => r.OnSunlightChanged());
        Messenger.RegisterPropertyChangedMessage(this, static (WorkforceManager x)   => x.Proportion,  static (r, m) => r.OnWorkerProportionChanged());
        Messenger.RegisterRequestMessage(this, static (r) => r._productCalculator.CalcNeedModules(r.Products, r._settings).ToArray());
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        Products.Clear();
        _modules.Modules.CollectionChanged -= OnModulesChanged;

        Messenger.UnregisterAll(this);
    }


    /// <summary>
    /// 単価を百分率ベースで一括設定する
    /// </summary>
    /// <param name="value">設定値</param>
    public void SetUnitPricePercent(long value)
    {
        foreach (var product in Products)
        {
            product.SetUnitPricePercent(value);
        }
    }


    /// <summary>
    /// 購入フラグを一括設定する
    /// </summary>
    /// <param name="value">設定値</param>
    /// <param name="onlySelectedItem">選択中の項目のみ設定するか</param>
    public void SetNoBuy(bool value, bool onlySelectedItem)
    {
        foreach (var prod in Products.Where(x => x.IsSelected || !onlySelectedItem))
        {
            prod.NoBuy = value;
        }
    }


    /// <summary>
    /// 販売フラグを一括設定
    /// </summary>
    /// <param name="value">設定値</param>
    /// <param name="onlySelectedItem">選択中の項目のみ設定するか</param>
    public void SetNoSell(bool value, bool onlySelectedItem)
    {
        foreach (var prod in Products.Where(x => x.IsSelected || !onlySelectedItem))
        {
            prod.NoSell = value;
        }
    }


    /// <summary>
    /// 選択されたアイテムの展開/折りたたみ状態を設定
    /// </summary>
    /// <param name="value">設定値</param>
    public void SetExpanded(bool value)
    {
        foreach (var item in Products.Where(x => x.IsSelected))
        {
            item.IsExpanded = value;
        }
    }


    /// <summary>
    /// 現在労働者数と必要労働者数の割合に変化があった場合
    /// </summary>
    private void OnWorkerProportionChanged()
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
    /// 日光に変化があった場合
    /// </summary>
    private void OnSunlightChanged()
    {
        foreach (var prod in Products)
        {
            prod.SetEfficiency("sunlight", _settings.Sunlight);
        }
    }


    /// <summary>
    /// モジュール数に変更があった場合
    /// </summary>
    private void OnModuleCountChanged(PropertyChangedMessage<long> message)
    {
        if (message.Sender is not ModulesGridItem module)
        {
            return;
        }

        // 生産/消費ウェアのどちらも無いモジュール以外は再計算不要のため何もしない
        if (!(module.Module.Resources.Any() || module.Module.Products.Any()))
        {
            return;
        }

        // 変更があったモジュールに対応する生産/消費ウェアに対して詳細情報を設定
        var prodDict = AggregateProduct([module]);
        foreach (var item in prodDict)
        {
            Products.FirstOrDefault(x => x.Ware.Equals(item.Key))?.SetDetails(item.Value, message.OldValue);
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
            using var prevOptions = Products.ToPooledDictionary(x => x.Ware.ID);
            Products.Clear();

            if (_modules.Modules.Any())
            {
                var products = AggregateProduct(_modules.Modules);

                // 可能なら前回値復元して製品一覧に追加
                var addItems = products.Select
                (
                    x =>
                    {
                        if (prevOptions.TryGetValue(x.Key.ID, out var oldProd))
                        {
                            return new ProductsGridItem(Messenger, x.Key, x.Value, oldProd.NoBuy, oldProd.NoSell, oldProd.UnitPrice, oldProd.EditStatus);
                        }

                        return new ProductsGridItem(Messenger, x.Key, x.Value);
                    }
                );

                _products.Products.AddRange(addItems);
            }
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
                addItems.Add(new ProductsGridItem(Messenger, item.Key, item.Value, false, false, -1, EditStatus.Edited));
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
                    .SelectMany(x => x.Select(y => CreateProductDetailsItem(y, x.Sum(z => z.ModuleCount), x.Sum(z => z.WareAmount))))
                    .ToArray() as IReadOnlyList<IProductDetailsListItem>
            );

    }


    /// <summary>
    /// 製品詳細情報を作成する
    /// </summary>
    /// <param name="calcResult">計算結果</param>
    /// <param name="moduleCount">モジュール数</param>
    /// <param name="wareAmount">生産/消費数</param>
    /// <returns>製品詳細情報</returns>
    private IProductDetailsListItem CreateProductDetailsItem(CalcResult calcResult, long moduleCount, long wareAmount)
    {
        if (calcResult.Efficiency is not null)
        {
            return new ProductDetailsListItem(calcResult.WareID, calcResult.Module, moduleCount, calcResult.Efficiency, wareAmount, _settings);
        }
        else
        {
            return new ProductDetailsListItemConsumption(calcResult.WareID, calcResult.Module, moduleCount, wareAmount);
        }
    }
}

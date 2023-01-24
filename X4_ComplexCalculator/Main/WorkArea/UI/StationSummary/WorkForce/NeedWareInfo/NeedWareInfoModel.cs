using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Products;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.WorkForce.NeedWareInfo;

/// <summary>
/// 必要ウェア情報
/// </summary>
class NeedWareInfoModel : BindableBase, IDisposable
{
    #region メンバ
    /// <summary>
    /// モジュール情報一覧
    /// </summary>
    private readonly IModulesInfo _Modules;


    /// <summary>
    /// 製品情報一覧
    /// </summary>
    private readonly IProductsInfo _Products;


    /// <summary>
    /// 集計対象ウェア数量ディクショナリ
    /// </summary>
    /// <remarks>
    /// Key   = ウェアID
    /// Value = 生産数
    /// 
    /// イベントの発火順番が前後する事を考慮したいため、集計対象のウェアの情報をここに格納しておく
    /// </remarks>
    private readonly Dictionary<string, long> AggregateTargetProducts;
    #endregion


    #region プロパティ
    /// <summary>
    /// 必要ウェア情報詳細
    /// </summary>
    public ObservableRangeCollection<NeedWareInfoDetailsItem> NeedWareInfoDetails { get; } = new();
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="modules">モジュール一覧情報</param>
    /// <param name="products">製品一覧情報</param>
    public NeedWareInfoModel(IModulesInfo modules, IProductsInfo products)
    {
        _Modules = modules;
        _Modules.Modules.CollectionChanged += Modules_CollectionChanged;
        _Modules.Modules.CollectionPropertyChanged += Modules_CollectionPropertyChanged;

        _Products = products;
        _Products.Products.CollectionChanged += Products_CollectionChanged;
        _Products.Products.CollectionPropertyChanged += Products_CollectionPropertyChanged;


        // 集計対象ウェアを取得
        AggregateTargetProducts = X4Database.Instance.Ware.Get("workunit_busy")
            .Resources
            .SelectMany(x => x.Value.Select(y => y.NeedWareID))
            .Distinct()
            .ToDictionary(x => x, x => 0L);
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _Modules.Modules.CollectionChanged -= Modules_CollectionChanged;
        _Modules.Modules.CollectionPropertyChanged -= Modules_CollectionPropertyChanged;
        _Products.Products.CollectionChanged -= Products_CollectionChanged;
        _Products.Products.CollectionPropertyChanged -= Products_CollectionPropertyChanged;
    }


    /// <summary>
    /// モジュールのプロパティ変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Modules_CollectionPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // ModulesGridItemでなければ何もしない
        if (sender is not ModulesGridItem module)
        {
            return;
        }

        // 居住モジュールでなければ何もしない
        if (module.Module.WorkersCapacity <= 0)
        {
            return;
        }

        // PropertyChangedExtendedEventArgsでない or モジュール数変更以外なら何もしない
        if (e is not PropertyChangedExtendedEventArgs<long> ev || e.PropertyName != nameof(ModulesGridItem.ModuleCount))
        {
            return;
        }
        
        // 必要ウェア集計
        var calcResults = ProductCalculator.Instance.Calc(module.Module, 1);
        foreach (var calcResultGrp in calcResults.GroupBy(x => x.Method))
        {
            foreach (var calcResult in calcResultGrp)
            {
                foreach (var item in NeedWareInfoDetails.Where(x => x.Method == calcResult.Method && x.WareID == calcResult.WareID))
                {
                    item.NeedAmount -= (ev.NewValue - ev.OldValue) * calcResult.WareAmount;
                }
            }
        }

        // 合計必要数量更新
        UpdateTotalNeedAmount();
    }


    /// <summary>
    /// モジュール一覧変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Modules_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var addWares = new Dictionary<string, Dictionary<string, long>>();

        // 削除予定のウェアを集計
        if (e.OldItems is not null)
        {
            var calcResults = e.OldItems.Cast<ModulesGridItem>()
                .Where(x => 0 < x.Module.WorkersCapacity)
                .SelectMany(x => ProductCalculator.Instance.Calc(x.Module, 1))
                .GroupBy(x => x.Method);

            foreach (var calcResultGrp in calcResults)
            {
                if (!addWares.ContainsKey(calcResultGrp.Key))
                {
                    addWares.Add(calcResultGrp.Key, new Dictionary<string, long>());
                }

                foreach (var calcResult in calcResultGrp)
                {
                    if (!addWares[calcResult.Method].ContainsKey(calcResult.WareID))
                    {
                        addWares[calcResult.Method].Add(calcResult.WareID, calcResult.WareAmount);
                    }
                    else
                    {
                        addWares[calcResult.Method][calcResult.WareID] += calcResult.WareAmount;
                    }
                }
            }
        }

        // 追加予定のウェアを集計
        if (e.NewItems is not null)
        {
            var calcResults = e.NewItems.Cast<ModulesGridItem>()
                .Where(x => 0 < x.Module.WorkersCapacity)
                .SelectMany(x => ProductCalculator.Instance.Calc(x.Module, 1))
                .GroupBy(x => x.Method);

            foreach (var calcResultGrp in calcResults)
            {
                if (!addWares.ContainsKey(calcResultGrp.Key))
                {
                    addWares.Add(calcResultGrp.Key, new Dictionary<string, long>());
                }

                foreach (var calcResult in calcResultGrp)
                {
                    if (!addWares[calcResult.Method].ContainsKey(calcResult.WareID))
                    {
                        addWares[calcResult.Method].Add(calcResult.WareID, -calcResult.WareAmount);
                    }
                    else
                    {
                        addWares[calcResult.Method][calcResult.WareID] += -calcResult.WareAmount;
                    }
                }
            }
        }

        // リセットされた場合
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            addWares.Clear();
            NeedWareInfoDetails.Clear();

            var calcResults = ((sender as IEnumerable<ModulesGridItem>) ?? throw new ArgumentException())
                .Where(x => 0 < x.Module.WorkersCapacity)
                .SelectMany(x => ProductCalculator.Instance.Calc(x.Module, 1))
                .GroupBy(x => x.Method);

            foreach (var calcResultGrp in calcResults)
            {
                if (!addWares.ContainsKey(calcResultGrp.Key))
                {
                    addWares.Add(calcResultGrp.Key, new Dictionary<string, long>());
                }

                foreach (var calcResult in calcResultGrp)
                {
                    if (!addWares[calcResult.Method].ContainsKey(calcResult.WareID))
                    {
                        addWares[calcResult.Method].Add(calcResult.WareID, calcResult.WareAmount);
                    }
                    else
                    {
                        addWares[calcResult.Method][calcResult.WareID] += calcResult.WareAmount;
                    }
                }
            }
        }

        // ウェア集計
        // 削除対象レコード
        var removeItems = new List<(string Method, string WareID)>();
        foreach (var (method, wareArr) in addWares)
        {
            foreach (var (wareID, amount) in wareArr)
            {
                var item = NeedWareInfoDetails.FirstOrDefault(x => x.Method == method && x.WareID == wareID);

                if (item is not null)
                {
                    item.NeedAmount += amount;

                    // 必要ウェア数が0なら削除対象に追加
                    if (item.NeedAmount == 0)
                    {
                        removeItems.Add((method, wareID));
                    }
                }
                else
                {
                    var race = X4Database.Instance.Race.TryGet((method == "default") ? "argon" : method);
                    if (race is not null)
                    {
                        NeedWareInfoDetails.Add(new NeedWareInfoDetailsItem(race, method, wareID, amount, AggregateTargetProducts[wareID]));
                    }
                }
            }
        }

        // 不要な要素を消す
        if (removeItems.Any())
        {
            // 居住モジュールの種族(メソッド)一覧
            var habModuleMethods = _Modules.Modules
                .Where(x => 0 < x.Module.WorkersCapacity)
                .Select(x =>
                {
                    var ret = x.Module.Owners.First().Race.RaceID;
                    return (ret == "argon") ? "default" : ret;
                })
                .Distinct()
                .ToArray();

            // モジュールがまだあるなら削除対象から除外する
            removeItems.RemoveAll(x => habModuleMethods.Any(y => x.Method == y));
            NeedWareInfoDetails.RemoveAll(x => removeItems.Any(y => x.Method == y.Method && x.WareID == y.WareID));
        }

        // 合計必要数量更新
        UpdateTotalNeedAmount();
    }


    /// <summary>
    /// 合計必要数量更新
    /// </summary>
    private void UpdateTotalNeedAmount()
    {
        var totalWares = NeedWareInfoDetails
            .GroupBy(x => x.WareID)
            .Select(x => (WareID: x.Key, Amount: x.Sum(y => y.NeedAmount)));
        foreach (var (WareID, Amount) in totalWares)
        {
            var items = NeedWareInfoDetails.Where(x => x.WareID == WareID);
            foreach (var item in items)
            {
                item.TotalNeedAmount = Amount;
            }
        }
    }


    /// <summary>
    /// 製品のプロパティ変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Products_CollectionPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // ウェア数量以外の変更なら何もしない
        if (e.PropertyName != nameof(ProductsGridItem.Count))
        {
            return;
        }

        // キャストに失敗したら何もしない
        if (sender is not ProductsGridItem product)
        {
            return;
        }


        // 同じウェアの数量更新
        var keys = AggregateTargetProducts.Keys.ToArray();
        foreach (var wareID in keys.Where(x => x == product.Ware.ID))
        {
            var amount = product.Details.Where(x => 0 < x.Amount).Sum(x => x.Amount);
            AggregateTargetProducts[wareID] = amount;

            // 現在の列にあればそちらを使用
            foreach (var itm in NeedWareInfoDetails.Where(x => x.WareID == wareID))
            {
                itm.ProductionAmount = amount;
            }
        }
    }


    /// <summary>
    /// 製品一覧変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Products_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        System.Collections.IList?[] items = { e.NewItems, e.OldItems };

        foreach (var item in items)
        {
            if (item is null)
            {
                continue;
            }


            foreach (var prod in item.Cast<ProductsGridItem>().Where(x => AggregateTargetProducts.ContainsKey(x.Ware.ID)))
            {
                var amount = prod.Details.Where(x => 0 < x.Amount).Sum(x => x.Amount);
                AggregateTargetProducts[prod.Ware.ID] = amount;

                var ware = NeedWareInfoDetails.FirstOrDefault(x => x.WareID == prod.Ware.ID);
                if (ware is not null)
                {
                    ware.ProductionAmount = amount;
                }
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            NeedWareInfoDetails.Clear();

            foreach (var key in AggregateTargetProducts.Keys.ToArray())
            {
                AggregateTargetProducts[key] = 0;
            }
        }
    }
}

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.Main.PlanningArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.PlanningArea.UI.ProductsGrid
{
    /// <summary>
    /// 製品一覧用DataGridViewのModel
    /// </summary>
    class ProductsGridModel : BindableBase, IDisposable
    {
        #region メンバ
        /// <summary>
        /// モジュール一覧
        /// </summary>
        private readonly ObservablePropertyChangedCollection<ModulesGridItem> _Modules;

        /// <summary>
        /// 生産性
        /// </summary>
        private double _Efficiency;

        /// <summary>
        /// 製品計算機
        /// </summary>
        private readonly ProductCalclator _ProductCalclator = ProductCalclator.Create();


        /// <summary>
        /// 単価保存用
        /// </summary>
        private readonly Dictionary<string, long> _UnitPriceBakDict = new Dictionary<string, long>();
        #endregion

        #region プロパティ
        /// <summary>
        /// 製品一覧
        /// </summary>
        public ObservablePropertyChangedCollection<ProductsGridItem> Products { get; private set; }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">モジュール一覧</param>
        public ProductsGridModel(ObservablePropertyChangedCollection<ModulesGridItem> modules)
        {
            Products = new ObservablePropertyChangedCollection<ProductsGridItem>();

            modules.CollectionChangedAsync += OnModulesChanged;
            modules.CollectionPropertyChangedAsync += OnModulePropertyChanged;

            _Modules = modules;
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            Products.Clear();
            _Modules.CollectionChangedAsync -= OnModulesChanged;
            _Modules.CollectionPropertyChangedAsync -= OnModulePropertyChanged;
        }


        /// <summary>
        /// モジュール自動追加
        /// </summary>
        public void AutoAddModule()
        {
            var result = Localize.ShowMessageBox("Lang:AutoAddConfirm", "Lang:Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var addedRecords = 0L;              // 追加レコード数
            var addedModules = 0L;              // 追加モジュール数

            while(true)
            {
                // 追加モジュール一覧
                var addModules = _ProductCalclator.CalcNeedModules(Products);

                if (!addModules.Any())
                {
                    break;
                }

                addedRecords += addModules.Count;
                addedModules += addModules.Sum(x => x.Count);

                _Modules.AddRange(addModules.Select(x => new ModulesGridItem(x.ModuleID, x.Count)));
            }

            if (addedRecords == 0)
            {
                Localize.ShowMessageBox("Lang:NoAddedModulesAutomaticallyMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Localize.ShowMessageBox("Lang:AddedModulesAutomaticallyMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, addedRecords, addedModules);
            }
        }


        /// <summary>
        /// モジュールのプロパティが変更された場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task OnModulePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // モジュール数変更時以外は処理しない
            if (e.PropertyName != "ModuleCount")
            {
                await Task.CompletedTask;
                return;
            }

            if(!(sender is ModulesGridItem module))
            {
                await Task.CompletedTask;
                return;
            }

            // 製造モジュールか居住モジュールの場合のみ更新
            if (0 < module.Module.MaxWorkers ||
                0 < module.Module.WorkersCapacity ||
                module.Module.ModuleType.ModuleTypeID == "production" ||
                module.Module.ModuleType.ModuleTypeID == "habitation"
                )
            {
                if (!(e is PropertyChangedExtendedEventArgs<long> ev))
                {
                    return;
                }

                OnModuleCountChanged(module, ev.OldValue);
                UpdateEfficiency();
            }

            await Task.CompletedTask;
        }


        /// <summary>
        /// 生産性を計算
        /// </summary>
        /// <returns>生産性</returns>
        private double CalcEfficiency()
        {
            var ret = 0.0;

            var maxWorkers = _Modules.Sum(x => x.Module.MaxWorkers * x.ModuleCount);
            var workersCapacity = _Modules.Sum(x => x.Module.WorkersCapacity * x.ModuleCount);

            // 生産性を0.0以上、1.0以下にする
            if (0 < workersCapacity)
            {
                ret = (maxWorkers < workersCapacity) ? 1.0 : (double)workersCapacity / maxWorkers;
            }

            return ret;
        }

        /// <summary>
        /// 製品更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task OnModulesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                OnModuleAdded(e.NewItems.Cast<ModulesGridItem>());
            }

            if (e.OldItems != null)
            {
                OnModuleRemoved(e.OldItems.Cast<ModulesGridItem>());
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // 単価保存
                foreach (var delProd in Products)
                {
                    _UnitPriceBakDict.Add(delProd.Ware.WareID, delProd.UnitPrice);
                }

                Products.Clear();
            }

            UpdateEfficiency();

            await Task.CompletedTask;
        }


        /// <summary>
        /// 生産性を更新
        /// </summary>
        private void UpdateEfficiency()
        {
            // 生産性(倍率)
            double efficiency = CalcEfficiency();

            // 生産性が変化しない場合、何もしない
            if (efficiency == _Efficiency)
            {
                return;
            }

            foreach (var prod in Products)
            {
                prod.SetEfficiency(efficiency);
            }

            _Efficiency = efficiency;
        }


        /// <summary>
        /// モジュール数変更時
        /// </summary>
        /// <param name="module">変更があったモジュール</param>
        /// <param name="prevModuleCount">変更前モジュール数</param>
        private void OnModuleCountChanged(ModulesGridItem module, long prevModuleCount)
        {
            ModulesGridItem[] modules = { module };

            Dictionary<string, List<ProductDetailsListItem>> prodDict = AggregateProduct(modules);

            foreach (var item in prodDict)
            {
                // 変更対象のウェアを検索
                Products.Where(x => x.Ware.WareID == item.Key).FirstOrDefault()?.SetDetails(item.Value, prevModuleCount);
            }
        }


        /// <summary>
        /// モジュールが追加された場合
        /// </summary>
        /// <param name="addedModules">追加されたモジュール</param>
        private void OnModuleAdded(IEnumerable<ModulesGridItem> addedModules)
        {
            // 生産品集計用ディクショナリ
            Dictionary<string, List<ProductDetailsListItem>> prodDict = AggregateProduct(addedModules);

            var addItems = new List<ProductsGridItem>();
            foreach (var item in prodDict)
            {
                // すでにウェアが存在するか検索
                var prod = Products.Where(x => x.Ware.WareID == item.Key).FirstOrDefault();
                if (prod != null)
                {
                    // ウェアが一覧にある場合
                    prod.AddDetails(item.Value);
                }
                else
                {
                    // ウェアが一覧に無い場合
                    prod = new ProductsGridItem(item.Key, item.Value);

                    // 前回値単価がある場合、復元
                    if (_UnitPriceBakDict.ContainsKey(prod.Ware.WareID))
                    {
                        prod.UnitPrice = _UnitPriceBakDict[prod.Ware.WareID];
                    }
                    addItems.Add(prod);
                }
            }

            // マージ処理以外で反応しないようにするためクリアする
            _UnitPriceBakDict.Clear();

            Products.AddRange(addItems);
        }


        /// <summary>
        /// モジュールが削除された場合
        /// </summary>
        /// <param name="removedModules">追加されたモジュール</param>
        private void OnModuleRemoved(IEnumerable<ModulesGridItem> removedModules)
        {
            // 生産品集計用ディクショナリ
            Dictionary<string, List<ProductDetailsListItem>> prodDict = AggregateProduct(removedModules);

            foreach (var item in prodDict)
            {
                // 一致するウェアの詳細情報を削除
                Products.Where(x => x.Ware.WareID == item.Key).FirstOrDefault()?.RemoveDetails(item.Value);
            }

            Products.RemoveAll(x => !x.Details.Any());
        }


        /// <summary>
        /// 製品情報を集計
        /// </summary>
        /// <param name="targetModules">集計対象モジュール</param>
        /// <returns>集計結果</returns>
        private Dictionary<string, List<ProductDetailsListItem>> AggregateProduct(IEnumerable<ModulesGridItem> targetModules)
        {
            // 生産品集計用ディクショナリ
            var prodDict = new Dictionary<string, List<ProductDetailsListItem>>();    // <ウェアID, 詳細情報>

            // 処理対象モジュール一覧
            var modules = targetModules.Where(x => 0 < x.Module.MaxWorkers || 
                                                   0 < x.Module.WorkersCapacity ||
                                                   x.Module.ModuleType.ModuleTypeID == "production" ||
                                                   x.Module.ModuleType.ModuleTypeID == "habitation")
                                       .GroupBy(x => x.Module.ModuleID)
                                       .Select(x =>
                                       {
                                           var module = x.First().Module;
                                           return (module.ModuleID, module.ModuleType.ModuleTypeID, Count: x.Sum(y => y.ModuleCount));
                                       });
                                      
            // 処理対象モジュールが無ければ何もしない
            if (!modules.Any())
            {
                return prodDict;
            }


            foreach (var module in modules)
            {
                switch (module.ModuleTypeID)
                {
                    // 製造モジュールの場合
                    case "production":
                        SumProduct(module.ModuleID, module.Count, prodDict);
                        break;

                    // 居住モジュールの場合
                    case "habitation":
                        SumHabitation(module.ModuleID, module.Count, prodDict);
                        break;

                    default:
                        break;
                }
            }

            return prodDict;
        }


        /// <summary>
        /// 製品を集計
        /// </summary>
        /// <param name="moduleID"></param>
        /// <param name="count"></param>
        /// <param name="prodDict"></param>
        private void SumProduct(string moduleID, long count, Dictionary<string, List<ProductDetailsListItem>> prodDict)
        {
            foreach (var ware in _ProductCalclator.CalcProduction(moduleID))
            {
                // ウェアがなければ追加する
                if (!prodDict.ContainsKey(ware.WareID))
                {
                    prodDict.Add(ware.WareID, new List<ProductDetailsListItem>());
                }

                
                var detailsList = prodDict[ware.WareID];
                var details = detailsList.Where(x => x.ModuleID == moduleID).FirstOrDefault();

                // モジュールが既に追加されている場合、モジュール数を増やしてレコードが少なくなるようにする
                if (details != null)
                {
                    details.Incriment(count);
                }
                else
                {
                    detailsList.Add(new ProductDetailsListItem(moduleID, count, ware.Efficiency, ware.Amount));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moduleID"></param>
        /// <param name="moduleCount">モジュール数</param>
        /// <param name="workers"></param>
        /// <param name="prodDict"></param>
        private void SumHabitation(string moduleID, long moduleCount, Dictionary<string, List<ProductDetailsListItem>> prodDict)
        {
            foreach (var ware in _ProductCalclator.CalcHabitation(moduleID))
            {
                if (!prodDict.ContainsKey(ware.WareID))
                {
                    prodDict.Add(ware.WareID, new List<ProductDetailsListItem>());
                }

                var detailsList = prodDict[ware.WareID];
                var details = detailsList.Where(x => x.ModuleID == moduleID).FirstOrDefault();
                if (details != null)
                {
                    details.Incriment(moduleCount);
                }
                else
                {
                    detailsList.Add(new ProductDetailsListItem(moduleID, moduleCount, -1, ware.Amount));
                }
            }
        }
    }
}

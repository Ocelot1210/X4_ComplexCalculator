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
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.StationSettings;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid
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
        /// ステーションの設定
        /// </summary>
        private readonly StationSettingsModel _Settings;


        /// <summary>
        /// 生産性
        /// </summary>
        private double _Efficiency;


        /// <summary>
        /// 製品計算機
        /// </summary>
        private readonly ProductCalculator _ProductCalculator = ProductCalculator.Instance;


        /// <summary>
        /// 単価保存用
        /// </summary>
        private readonly Dictionary<string, long> _UnitPriceBakDict = new Dictionary<string, long>();


        /// <summary>
        /// 売買オプション保存用
        /// </summary>
        private readonly Dictionary<string, TradeOption> _TradeOptionDict = new Dictionary<string, TradeOption>();
        #endregion


        #region プロパティ
        /// <summary>
        /// 製品一覧
        /// </summary>
        public ObservablePropertyChangedCollection<ProductsGridItem> Products { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modules">モジュール一覧</param>
        /// <param name="settings">ステーションの設定</param>
        public ProductsGridModel(ObservablePropertyChangedCollection<ModulesGridItem> modules, StationSettingsModel settings)
        {
            Products = new ObservablePropertyChangedCollection<ProductsGridItem>();

            modules.CollectionChangedAsync += OnModulesChanged;
            modules.CollectionPropertyChangedAsync += OnModulePropertyChanged;

            _Modules = modules;
            _Settings = settings;
            _Settings.PropertyChanged += Settings_PropertyChanged;
            _Settings.Workforce.PropertyChanged += Workforce_PropertyChanged;
        }



        /// <summary>
        /// 労働者情報に変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Workforce_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // 日光
                case nameof(StationSettingsModel.Sunlight):
                    foreach (var prod in Products)
                    {
                        prod.SetEfficiency("sunlight", _Settings.Sunlight);
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
            _Modules.CollectionChangedAsync -= OnModulesChanged;
            _Modules.CollectionPropertyChangedAsync -= OnModulePropertyChanged;
            _Settings.PropertyChanged -= Settings_PropertyChanged;
            _Settings.Workforce.PropertyChanged -= Workforce_PropertyChanged;
        }


        /// <summary>
        /// モジュール自動追加
        /// </summary>
        public void AutoAddModule()
        {
            var result = LocalizedMessageBox.Show("Lang:AutoAddConfirm", "Lang:Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var addedRecords = 0L;              // 追加レコード数
            var addedModules = 0L;              // 追加モジュール数

            while (true)
            {
                // 追加モジュール一覧
                var addModules = _ProductCalculator.CalcNeedModules(Products, _Settings);

                // 追加モジュールが無ければ(不足が無くなれば)終了
                if (!addModules.Any())
                {
                    break;
                }

                // 追加レコード数とモジュール数を更新
                addedRecords += addModules.Count;
                addedModules += addModules.Sum(x => x.Count);

                // モジュール一覧に追加対象モジュールを追加
                _Modules.AddRange(addModules.Select(x => new ModulesGridItem(x.ModuleID, x.Count)));
            }


            if (addedRecords == 0)
            {
                LocalizedMessageBox.Show("Lang:NoAddedModulesAutomaticallyMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                LocalizedMessageBox.Show("Lang:AddedModulesAutomaticallyMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, addedRecords, addedModules);
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

            if (!(sender is ModulesGridItem module))
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
            }

            await Task.CompletedTask;
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
                OnModuleAdded(_Modules);
            }

            await Task.CompletedTask;
        }


        /// <summary>
        /// 生産性を更新
        /// </summary>
        private void UpdateWorkerEfficiency()
        {
            // 労働者による生産性(倍率)
            double efficiency = _Settings.Workforce.Proportion;

            if (1.0 < efficiency)
            {
                efficiency = 1.0;
            }

            // 労働による生産性が変化しない場合、何もしない
            if (efficiency == _Efficiency)
            {
                return;
            }

            foreach (var prod in Products)
            {
                prod.SetEfficiency("work", efficiency);
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

            Dictionary<string, List<IProductDetailsListItem>> prodDict = AggregateProduct(modules);

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
            Dictionary<string, List<IProductDetailsListItem>> prodDict = AggregateProduct(addedModules);

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

                    // 売買オプション前回値があれば取得する
                    if (!_TradeOptionDict.TryGetValue(item.Key, out var option))
                    {
                        // 前回値がない場合
                        option = new TradeOption();
                        _TradeOptionDict.Add(item.Key, option);
                    }

                    prod = new ProductsGridItem(item.Key, item.Value, option);

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
            Dictionary<string, List<IProductDetailsListItem>> prodDict = AggregateProduct(removedModules);

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
        private Dictionary<string, List<IProductDetailsListItem>> AggregateProduct(IEnumerable<ModulesGridItem> targetModules)
        {
            // 生産品集計用ディクショナリ
            var prodDict = new Dictionary<string, List<IProductDetailsListItem>>();    // <ウェアID, 詳細情報>

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
        private void SumProduct(string moduleID, long count, Dictionary<string, List<IProductDetailsListItem>> prodDict)
        {
            foreach (var ware in _ProductCalculator.CalcProduction(moduleID))
            {
                // ウェアがなければ追加する
                if (!prodDict.ContainsKey(ware.WareID))
                {
                    prodDict.Add(ware.WareID, new List<IProductDetailsListItem>());
                }


                var detailsList = prodDict[ware.WareID];
                var details = detailsList.Where(x => x.ModuleID == moduleID).FirstOrDefault();

                // モジュールが既に追加されている場合、モジュール数を増やしてレコードが少なくなるようにする
                if (details != null)
                {
                    details.ModuleCount += count;
                }
                else
                {
                    if (ware.Efficiency != null)
                    {
                        detailsList.Add(new ProductDetailsListItem(moduleID, count, ware.Efficiency, ware.Amount, _Settings));
                    }
                    else
                    {
                        detailsList.Add(new ProductDetailsListItemConsumption(moduleID, count, ware.Amount));
                    }
                }
            }
        }


        /// <summary>
        /// 居住モジュールを集計
        /// </summary>
        /// <param name="moduleID"></param>
        /// <param name="moduleCount">モジュール数</param>
        /// <param name="workers"></param>
        /// <param name="prodDict"></param>
        private void SumHabitation(string moduleID, long moduleCount, Dictionary<string, List<IProductDetailsListItem>> prodDict)
        {
            foreach (var ware in _ProductCalculator.CalcHabitation(moduleID))
            {
                if (!prodDict.ContainsKey(ware.WareID))
                {
                    prodDict.Add(ware.WareID, new List<IProductDetailsListItem>());
                }

                var detailsList = prodDict[ware.WareID];
                var details = detailsList.Where(x => x.ModuleID == moduleID).FirstOrDefault();
                if (details != null)
                {
                    details.ModuleCount += moduleCount;
                }
                else
                {
                    detailsList.Add(new ProductDetailsListItemConsumption(moduleID, moduleCount, ware.Amount));
                }
            }
        }
    }
}

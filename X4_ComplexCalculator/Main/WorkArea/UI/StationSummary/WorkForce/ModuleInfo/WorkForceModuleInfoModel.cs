using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.StationSettings;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.WorkForce.ModuleInfo
{
    class WorkForceModuleInfoModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 必要な労働者数
        /// </summary>
        private long _NeedWorkforce = 0;


        /// <summary>
        /// 現在の労働者数
        /// </summary>
        private long _WorkForce = 0;


        /// <summary>
        /// モジュール一覧
        /// </summary>
        private readonly ObservablePropertyChangedCollection<ModulesGridItem> _Modules;


        /// <summary>
        /// ステーションの設定
        /// </summary>
        private readonly StationSettingsModel _Settings;


        /// <summary>
        /// 本部モジュール用データ
        /// </summary>
        private static readonly WorkForceModuleInfoDetailsItem _HQ = new WorkForceModuleInfoDetailsItem("module_player_prod_hq_01_macro", 1, StationSettingsModel.HQ_WORKERS, 0);
        #endregion


        #region プロパティ
        /// <summary>
        /// 労働力の詳細情報
        /// </summary>
        public ObservableRangeCollection<WorkForceModuleInfoDetailsItem> WorkForceDetails { get; private set; } = new ObservableRangeCollection<WorkForceModuleInfoDetailsItem>();

        /// <summary>
        /// 必要な労働者数
        /// </summary>
        public long NeedWorkforce
        {
            get => _NeedWorkforce;
            set => SetProperty(ref _NeedWorkforce, value);
        }


        /// <summary>
        /// 現在の労働者数
        /// </summary>
        public long WorkForce
        {
            get => _WorkForce;
            set => SetProperty(ref _WorkForce, value);
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">モジュール一覧のModel</param>
        public WorkForceModuleInfoModel(ObservablePropertyChangedCollection<ModulesGridItem> modules, StationSettingsModel settings)
        {
            _Modules = modules;
            _Modules.CollectionChangedAsync += OnModulesChanged;
            _Modules.CollectionPropertyChangedAsync += OnModulesPropertyChanged;

            _Settings = settings;
            _Settings.PropertyChanged += Settings_PropertyChanged;
        }


        /// <summary>
        /// ステーションの設定変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(StationSettingsModel.IsHeadquarters):
                    {
                        if (_Settings.IsHeadquarters)
                        {
                            NeedWorkforce += StationSettingsModel.HQ_WORKERS;
                            WorkForceDetails.Add(_HQ);
                        }
                        else
                        {
                            NeedWorkforce -= StationSettingsModel.HQ_WORKERS;
                            WorkForceDetails.Remove(_HQ);
                        }
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
            _Modules.CollectionChangedAsync -= OnModulesChanged;
            _Modules.CollectionPropertyChangedAsync -= OnModulesPropertyChanged;
            _Settings.PropertyChanged -= Settings_PropertyChanged;
            WorkForceDetails.Clear();
        }


        /// <summary>
        /// モジュールのプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task OnModulesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // モジュール数変更時以外は処理しない
            if (e.PropertyName != nameof(ModulesGridItem.ModuleCount))
            {
                await Task.CompletedTask;
                return;
            }

            if (!(sender is ModulesGridItem module))
            {
                await Task.CompletedTask;
                return;
            }

            // 労働力が必要なモジュールの場合
            if (0 < module.Module.MaxWorkers)
            {
                // 変更があったモジュールのレコードを検索
                var itm = WorkForceDetails.Where(x => x.ModuleID == module.Module.ModuleID).First();

                // 必要労働力を更新
                NeedWorkforce = NeedWorkforce - Math.Abs(itm.TotalWorkforce) + module.Module.MaxWorkers * module.ModuleCount;

                // モジュール数を更新
                itm.ModuleCount = module.ModuleCount;
            }


            // 労働者を収容できるモジュールの場合
            if (0 < module.Module.WorkersCapacity)
            {
                // 変更があったモジュールのレコードを検索
                var itm = WorkForceDetails.Where(x => x.ModuleID == module.Module.ModuleID).First();

                // 現在の労働者数を更新
                WorkForce = WorkForce - Math.Abs(itm.TotalWorkforce) + module.Module.WorkersCapacity * module.ModuleCount;

                // モジュール数を更新
                itm.ModuleCount = module.ModuleCount;
            }

            await Task.CompletedTask;
        }


        /// <summary>
        /// モジュール一覧変更時
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
                WorkForceDetails.Clear();
                NeedWorkforce = 0;
                WorkForce = 0;
                OnModuleAdded(_Modules);
            }

            await Task.CompletedTask;
        }


        /// <summary>
        /// モジュール追加時
        /// </summary>
        /// <param name="modules">追加モジュール一覧</param>
        private void OnModuleAdded(IEnumerable<ModulesGridItem> modules)
        {
            var details = modules.Where(x => 0 < x.Module.MaxWorkers || 0 < x.Module.WorkersCapacity)
                                 .GroupBy(x => x.Module.ModuleID)
                                 .Select(x => (x.First().Module, ModuleCount: x.Sum(y => y.ModuleCount)))
                                 .OrderBy(x => x.Module.Name);

            var needWorkforce = 0L;
            var workForce = 0L;

            var addItems = new List<WorkForceModuleInfoDetailsItem>();
            foreach (var d in details)
            {
                var itm = WorkForceDetails.Where(x => x.ModuleID == d.Module.ModuleID).FirstOrDefault();
                if (itm != null)
                {
                    if (0 < itm.WorkForce)
                    {
                        workForce += d.ModuleCount * itm.WorkForce;
                    }
                    else
                    {
                        needWorkforce += d.ModuleCount * itm.MaxWorkers;
                    }

                    itm.ModuleCount += d.ModuleCount;
                }
                else
                {
                    addItems.Add(new WorkForceModuleInfoDetailsItem(d.Module, d.ModuleCount));

                    if (0 < d.Module.WorkersCapacity)
                    {
                        workForce += d.Module.WorkersCapacity * d.ModuleCount;
                    }
                    else
                    {
                        needWorkforce += d.Module.MaxWorkers * d.ModuleCount;
                    }
                }
            }

            WorkForceDetails.AddRange(addItems);
            NeedWorkforce += needWorkforce;
            WorkForce += workForce;
        }


        /// <summary>
        /// モジュール削除時
        /// </summary>
        /// <param name="modules">削除モジュール一覧</param>
        private void OnModuleRemoved(IEnumerable<ModulesGridItem> modules)
        {
            var details = modules.Where(x => 0 < x.Module.MaxWorkers || 0 < x.Module.WorkersCapacity)
                                 .GroupBy(x => x.Module.ModuleID)
                                 .Select(x => (x.First().Module, ModuleCount: x.Sum(y => y.ModuleCount)))
                                 .OrderBy(x => x.Module.Name);

            var needWorkforce = 0L;
            var workForce = 0L;

            foreach (var d in details)
            {
                var itm = WorkForceDetails.Where(x => x.ModuleID == d.Module.ModuleID).FirstOrDefault();
                if (itm != null)
                {
                    if (0 < itm.WorkForce)
                    {
                        workForce += d.ModuleCount * itm.WorkForce;
                    }
                    else
                    {
                        needWorkforce += d.ModuleCount * itm.MaxWorkers;
                    }

                    itm.ModuleCount -= d.ModuleCount;
                }
            }


            NeedWorkforce -= needWorkforce;
            WorkForce -= workForce;

            WorkForceDetails.RemoveAll(x => x.ModuleCount == 0);
        }
    }
}

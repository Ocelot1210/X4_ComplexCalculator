using Prism.Mvvm;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.ModulesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.StationSummary.WorkForce.ModuleInfo
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
        readonly ObservablePropertyChangedCollection<ModulesGridItem> Modules;
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
        public WorkForceModuleInfoModel(ObservablePropertyChangedCollection<ModulesGridItem> modules)
        {
            Modules = modules;
            Modules.CollectionChangedAsync += OnModulesChanged;
            Modules.CollectionPropertyChangedAsync += OnModulesPropertyChanged;
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            Modules.CollectionChangedAsync -= OnModulesChanged;
            Modules.CollectionPropertyChangedAsync -= OnModulesPropertyChanged;
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
            UpdateWorkFource();
            await Task.CompletedTask;
        }


        /// <summary>
        /// 労働力情報を更新
        /// </summary>
        private void UpdateWorkFource()
        {
            var needWorkforce = 0L;
            var workforce = 0L;

            var details = Modules.Where(x => 0 < x.Module.MaxWorkers || 0 < x.Module.WorkersCapacity)
                                 .GroupBy(x => x.Module.ModuleID)
                                 .Select(x => new WorkForceModuleInfoDetailsItem(x.First().Module, x.Sum(y => y.ModuleCount)))
                                 .OrderBy(x => x.ModuleName);

            // 値を更新
            WorkForceDetails.Reset(details);

            foreach (var item in WorkForceDetails)
            {
                if (item.TotalWorkforce < 0)
                {
                    needWorkforce -= item.TotalWorkforce;
                }
                else
                {
                    workforce += item.TotalWorkforce;
                }
            }
            NeedWorkforce = needWorkforce;
            WorkForce = workforce;
        }
    }
}

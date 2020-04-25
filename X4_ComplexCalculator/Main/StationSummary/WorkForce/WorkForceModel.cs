using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.ModulesGrid;
using System.ComponentModel;

namespace X4_ComplexCalculator.Main.StationSummary.WorkForce
{
    class WorkForceModel : INotifyPropertyChangedBace
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
        readonly IReadOnlyCollection<ModulesGridItem> Modules;
        #endregion


        #region プロパティ
        /// <summary>
        /// 労働力の詳細情報
        /// </summary>
        public SmartCollection<WorkForceDetailsItem> WorkForceDetails { get; private set; } = new SmartCollection<WorkForceDetailsItem>();

        /// <summary>
        /// 必要な労働者数
        /// </summary>
        public long NeedWorkforce
        {
            get
            {
                return _NeedWorkforce;
            }
            set
            {
                if (_NeedWorkforce != value)
                {
                    _NeedWorkforce = value;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 現在の労働者数
        /// </summary>
        public long WorkForce
        {
            get
            {
                return _WorkForce;
            }
            set
            {
                if (_WorkForce != value)
                {
                    _WorkForce = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">モジュール一覧のModel</param>
        public WorkForceModel(MemberChangeDetectCollection<ModulesGridItem> modules)
        {
            Modules = modules;
            modules.OnCollectionChangedAsync += OnModulesChanged;
            modules.OnCollectionPropertyChangedAsync += OnModulesPropertyChanged;
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
            if (module.Module.ModuleType.ModuleTypeID == "production" ||
                module.Module.ModuleType.ModuleTypeID == "habitation"
                )
            {
                var itm = WorkForceDetails.Where(x => x.ModuleID == module.Module.ModuleID).First();

                if (0 < itm.TotalWorkforce)
                {
                    WorkForce = WorkForce - Math.Abs(itm.TotalWorkforce) + module.Module.WorkersCapacity * module.ModuleCount;
                }
                else
                {
                    
                    NeedWorkforce = NeedWorkforce - Math.Abs(itm.TotalWorkforce) + module.Module.MaxWorkers * module.ModuleCount;
                }
                
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

            var details = Modules.Where(x => x.Module.ModuleType.ModuleTypeID == "production" || x.Module.ModuleType.ModuleTypeID == "habitation")
                                 .GroupBy(x => x.Module.ModuleID)
                                 .Select(x =>
                                 {
                                     var ret = new WorkForceDetailsItem(x.First().Module, x.Sum(y => y.ModuleCount));
                                     needWorkforce += ret.MaxWorkers * ret.ModuleCount;
                                     workforce += ret.WorkersCapacity * ret.ModuleCount;
                                     return ret;
                                 })
                                 .OrderBy(x => x.ModuleName);

            // 値を更新
            WorkForceDetails.Reset(details);
            NeedWorkforce = needWorkforce;
            WorkForce = workforce;
        }
    }
}

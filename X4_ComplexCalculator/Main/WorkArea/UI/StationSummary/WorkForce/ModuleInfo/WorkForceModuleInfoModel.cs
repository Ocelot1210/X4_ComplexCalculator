using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Prism.Mvvm;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.WorkForce.ModuleInfo
{
    /// <summary>
    /// 労働力用モジュール情報用Model
    /// </summary>
    class WorkForceModuleInfoModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// モジュール一覧情報
        /// </summary>
        private readonly IModulesInfo _Modules;


        /// <summary>
        /// ステーションの設定
        /// </summary>
        private readonly IStationSettings _Settings;


        /// <summary>
        /// 本部モジュール用データ
        /// </summary>
        private readonly WorkForceModuleInfoDetailsItem _HQ;
        #endregion


        #region プロパティ
        /// <summary>
        /// 労働力の詳細情報
        /// </summary>
        public ObservableRangeCollection<WorkForceModuleInfoDetailsItem> WorkForceDetails { get; } = new();
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modules">モジュール一覧情報</param>
        /// <param name="settings">ステーションの設定</param>
        public WorkForceModuleInfoModel(IModulesInfo modules, IStationSettings settings)
        {
            _Modules = modules;
            _Modules.Modules.CollectionChangedAsync += OnModulesChanged;
            _Modules.Modules.CollectionPropertyChangedAsync += OnModulesPropertyChanged;

            _Settings = settings;
            _Settings.PropertyChanged += Settings_PropertyChanged;
            _HQ = new WorkForceModuleInfoDetailsItem("module_player_prod_hq_01_macro", 1, _Settings.HQWorkers, 0);
        }


        /// <summary>
        /// ステーションの設定変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IStationSettings.IsHeadquarters):
                    {
                        if (_Settings.IsHeadquarters)
                        {
                            _Settings.Workforce.Need += _HQ.MaxWorkers;
                            WorkForceDetails.Add(_HQ);
                        }
                        else
                        {
                            _Settings.Workforce.Need -= _HQ.MaxWorkers;
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
            _Modules.Modules.CollectionChangedAsync -= OnModulesChanged;
            _Modules.Modules.CollectionPropertyChangedAsync -= OnModulesPropertyChanged;
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

            if (sender is not ModulesGridItem module)
            {
                await Task.CompletedTask;
                return;
            }

            // 労働力が必要なモジュールの場合
            if (0 < module.Module.MaxWorkers)
            {
                // 変更があったモジュールのレコードを検索
                var itm = WorkForceDetails.First(x => x.ModuleID == module.Module.ModuleID);

                // 必要労働力を更新
                _Settings.Workforce.Need = _Settings.Workforce.Need - Math.Abs(itm.TotalWorkforce) + module.Module.MaxWorkers * module.ModuleCount;

                // モジュール数を更新
                itm.ModuleCount = module.ModuleCount;
            }


            // 労働者を収容できるモジュールの場合
            if (0 < module.Module.WorkersCapacity)
            {
                // 変更があったモジュールのレコードを検索
                var itm = WorkForceDetails.First(x => x.ModuleID == module.Module.ModuleID);

                // 現在の労働者数を更新
                _Settings.Workforce.Capacity = _Settings.Workforce.Capacity - Math.Abs(itm.TotalWorkforce) + module.Module.WorkersCapacity * module.ModuleCount;

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
        private async Task OnModulesChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
                _Settings.Workforce.Clear();
                OnModuleAdded(_Modules.Modules);

                // 本部なら本部モジュールを追加
                if (_Settings.IsHeadquarters)
                {
                    _Settings.Workforce.Need += _HQ.MaxWorkers;
                    WorkForceDetails.Add(_HQ);
                }
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
            var capacity = 0L;

            var addItems = new List<WorkForceModuleInfoDetailsItem>();
            foreach (var d in details)
            {
                var itm = WorkForceDetails.FirstOrDefault(x => x.ModuleID == d.Module.ModuleID);
                if (itm != null)
                {
                    if (0 < itm.WorkForce)
                    {
                        capacity += d.ModuleCount * itm.WorkForce;
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
                        capacity += d.Module.WorkersCapacity * d.ModuleCount;
                    }
                    else
                    {
                        needWorkforce += d.Module.MaxWorkers * d.ModuleCount;
                    }
                }
            }

            WorkForceDetails.AddRange(addItems);
            _Settings.Workforce.Need += needWorkforce;
            _Settings.Workforce.Capacity += capacity;
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
            var capacity = 0L;

            foreach (var d in details)
            {
                var itm = WorkForceDetails.FirstOrDefault(x => x.ModuleID == d.Module.ModuleID);
                if (itm != null)
                {
                    if (0 < itm.WorkForce)
                    {
                        capacity += d.ModuleCount * itm.WorkForce;
                    }
                    else
                    {
                        needWorkforce += d.ModuleCount * itm.MaxWorkers;
                    }

                    itm.ModuleCount -= d.ModuleCount;
                }
            }

            _Settings.Workforce.Need -= needWorkforce;
            _Settings.Workforce.Capacity -= capacity;

            WorkForceDetails.RemoveAll(x => x.ModuleCount == 0);
        }
    }
}

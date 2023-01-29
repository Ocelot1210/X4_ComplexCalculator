using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.WorkForce.ModuleInfo;

/// <summary>
/// 労働力用モジュール情報用Model
/// </summary>
class WorkForceModuleInfoModel : BindableBase
{
    #region メンバ
    /// <summary>
    /// モジュール一覧情報
    /// </summary>
    private readonly IModulesInfo _modules;


    /// <summary>
    /// ステーションの設定
    /// </summary>
    private readonly IStationSettings _settings;


    /// <summary>
    /// 本部モジュール用データ
    /// </summary>
    private readonly WorkForceModuleInfoDetailsItem _hQ;
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
        _modules = modules;
        _modules.Modules.CollectionChangedAsync += OnModulesChanged;
        _modules.Modules.CollectionPropertyChangedAsync += OnModulesPropertyChanged;

        _settings = settings;
        _settings.PropertyChanged += Settings_PropertyChanged;
        _hQ = new WorkForceModuleInfoDetailsItem("module_player_prod_hq_01_macro", 1, _settings.HQWorkers, 0);
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
                    if (_settings.IsHeadquarters)
                    {
                        _settings.Workforce.Need += _hQ.MaxWorkers;
                        WorkForceDetails.Add(_hQ);
                    }
                    else
                    {
                        _settings.Workforce.Need -= _hQ.MaxWorkers;
                        WorkForceDetails.Remove(_hQ);
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
        _modules.Modules.CollectionChangedAsync -= OnModulesChanged;
        _modules.Modules.CollectionPropertyChangedAsync -= OnModulesPropertyChanged;
        _settings.PropertyChanged -= Settings_PropertyChanged;
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
            var itm = WorkForceDetails.First(x => x.ModuleID == module.Module.ID);

            // 必要労働力を更新
            _settings.Workforce.Need = _settings.Workforce.Need - Math.Abs(itm.TotalWorkforce) + module.Module.MaxWorkers * module.ModuleCount;

            // モジュール数を更新
            itm.ModuleCount = module.ModuleCount;
        }


        // 労働者を収容できるモジュールの場合
        if (0 < module.Module.WorkersCapacity)
        {
            // 変更があったモジュールのレコードを検索
            var itm = WorkForceDetails.First(x => x.ModuleID == module.Module.ID);

            // 現在の労働者数を更新
            _settings.Workforce.Capacity = _settings.Workforce.Capacity - Math.Abs(itm.TotalWorkforce) + module.Module.WorkersCapacity * module.ModuleCount;

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
            WorkForceDetails.Clear();
            _settings.Workforce.Clear();
            OnModuleAdded(_modules.Modules);

            // 本部なら本部モジュールを追加
            if (_settings.IsHeadquarters)
            {
                _settings.Workforce.Need += _hQ.MaxWorkers;
                WorkForceDetails.Add(_hQ);
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
                             .GroupBy(x => x.Module.ID)
                             .Select(x => (x.First().Module, ModuleCount: x.Sum(y => y.ModuleCount)))
                             .OrderBy(x => x.Module.Name);

        var needWorkforce = 0L;
        var capacity = 0L;

        var addItems = new List<WorkForceModuleInfoDetailsItem>();
        foreach (var (module, moduleCount) in details)
        {
            var itm = WorkForceDetails.FirstOrDefault(x => x.ModuleID == module.ID);
            if (itm is not null)
            {
                if (0 < itm.WorkForce)
                {
                    capacity += moduleCount * itm.WorkForce;
                }
                else
                {
                    needWorkforce += moduleCount * itm.MaxWorkers;
                }

                itm.ModuleCount += moduleCount;
            }
            else
            {
                addItems.Add(new WorkForceModuleInfoDetailsItem(module, moduleCount));

                if (0 < module.WorkersCapacity)
                {
                    capacity += module.WorkersCapacity * moduleCount;
                }
                else
                {
                    needWorkforce += module.MaxWorkers * moduleCount;
                }
            }
        }

        WorkForceDetails.AddRange(addItems);
        _settings.Workforce.Need += needWorkforce;
        _settings.Workforce.Capacity += capacity;
    }


    /// <summary>
    /// モジュール削除時
    /// </summary>
    /// <param name="modules">削除モジュール一覧</param>
    private void OnModuleRemoved(IEnumerable<ModulesGridItem> modules)
    {
        var details = modules.Where(x => 0 < x.Module.MaxWorkers || 0 < x.Module.WorkersCapacity)
                             .GroupBy(x => x.Module.ID)
                             .Select(x => (x.First().Module, ModuleCount: x.Sum(y => y.ModuleCount)))
                             .OrderBy(x => x.Module.Name);

        var needWorkforce = 0L;
        var capacity = 0L;

        foreach (var (module, moduleCount) in details)
        {
            var itm = WorkForceDetails.FirstOrDefault(x => x.ModuleID == module.ID);
            if (itm is not null)
            {
                if (0 < itm.WorkForce)
                {
                    capacity += moduleCount * itm.WorkForce;
                }
                else
                {
                    needWorkforce += moduleCount * itm.MaxWorkers;
                }

                itm.ModuleCount -= moduleCount;
            }
        }

        _settings.Workforce.Need -= needWorkforce;
        _settings.Workforce.Capacity -= capacity;

        WorkForceDetails.RemoveAll(x => x.ModuleCount == 0);
    }
}

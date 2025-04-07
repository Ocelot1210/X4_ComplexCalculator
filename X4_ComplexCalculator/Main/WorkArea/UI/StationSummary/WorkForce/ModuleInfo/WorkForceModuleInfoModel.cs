using Collections.Pooled;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collections;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.WorkForce.ModuleInfo;

/// <summary>
/// 労働力用モジュール情報用Model
/// </summary>
sealed partial class WorkForceModuleInfoModel : ObservableRecipient
{
    #region メンバ
    /// <summary>
    /// モジュール一覧情報
    /// </summary>
    private readonly ModulesInfo _modules;


    /// <summary>
    /// ステーションの設定
    /// </summary>
    private readonly StationSettingInfo _settings;


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
    public WorkForceModuleInfoModel(IMessenger messenger, ModulesInfo modules, StationSettingInfo settings) : base(messenger)
    {
        _modules = modules;
        _modules.Modules.CollectionChanged += OnModulesChanged;
        Messenger.RegisterPropertyChangedMessage(this, static (ModulesGridItem x) => x.ModuleCount, static (r, m) => r.OnModuleCountChanged(m));

        _settings = settings;
        Messenger.RegisterPropertyChangedMessage(this, static (StationSettingInfo x) => x.IsHeadquarters, static (r, m) => r.OnSettingsIsHeadquarterChanged(m));

        _hQ = new WorkForceModuleInfoDetailsItem("module_player_prod_hq_01_macro", 1, _settings.HQWorkers, 0);
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _modules.Modules.CollectionChanged -= OnModulesChanged;
        WorkForceDetails.Clear();
    }


    /// <summary>
    /// 本部かどうかが変更時
    /// </summary>
    private void OnSettingsIsHeadquarterChanged(PropertyChangedMessage<bool> message)
    {
        if (message.NewValue)
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


    /// <summary>
    /// モジュールのプロパティ変更時
    /// </summary>
    private void OnModuleCountChanged(PropertyChangedMessage<long> message)
    {
        if (message.Sender is not ModulesGridItem module)
        {
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
    }


    /// <summary>
    /// モジュール一覧変更時
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
    }


    /// <summary>
    /// モジュール追加時
    /// </summary>
    /// <param name="modules">追加モジュール一覧</param>
    private void OnModuleAdded(IEnumerable<ModulesGridItem> modules)
    {
        var details = modules
            .Where(x => 0 < x.Module.MaxWorkers || 0 < x.Module.WorkersCapacity)
            .GroupBy(x => x.Module.ID)
            .Select(x => (x.First().Module, ModuleCount: x.Sum(y => y.ModuleCount)))
            .OrderBy(x => x.Module.Name);

        var needWorkforce = 0L;
        var capacity = 0L;

        using var addItems = new PooledList<WorkForceModuleInfoDetailsItem>();
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
        var details = modules
            .Where(x => 0 < x.Module.MaxWorkers || 0 < x.Module.WorkersCapacity)
            .GroupBy(x => x.Module.ID)
            .Select(x => (x.First().Module, ModuleCount: x.Sum(y => y.ModuleCount)))
            .OrderBy(x => x.Module.Name);

        var needWorkforce = 0L;
        var capacity = 0L;

        var removeTargets = new PooledSet<WorkForceModuleInfoDetailsItem>();

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
                if (itm.ModuleCount == 0)
                {
                    removeTargets.Add(itm);
                }
            }
        }

        _settings.Workforce.Need -= needWorkforce;
        _settings.Workforce.Capacity -= capacity;

        // RemoveAll(x => x.ModuleCount == 0); とした場合、
        // 無関係なモジュール数 0 の項目まで削除されるので HashSet で覚えたやつだけ削除する
        WorkForceDetails.RemoveAll(x => removeTargets.Contains(x));
    }
}

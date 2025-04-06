using Collections.Pooled;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collections;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.BuildResources;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;

namespace X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;

/// <summary>
/// 建造に必要なリソースを表示するDataGridView用Model
/// </summary>
sealed class BuildResourcesGridModel : ObservableRecipient, IDisposable
{
    #region メンバ
    /// <summary>
    /// モジュール一覧
    /// </summary>
    private readonly ModulesInfo _modules;


    /// <summary>
    /// 建造リソース情報
    /// </summary>
    private readonly BuildResourcesInfo _buildResources;


    /// <summary>
    /// 建造リソース計算用
    /// </summary>
    private readonly BuildResourceCalculator _calculator = BuildResourceCalculator.Instance;
    #endregion


    #region プロパティ
    /// <summary>
    /// 建造に必要なリソース
    /// </summary>
    public ObservableRangeCollection<BuildResourcesGridItem> Resources => _buildResources.BuildResources;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="modules">モジュール一覧情報</param>
    /// <param name="buildResources">建造リソース情報</param>
    public BuildResourcesGridModel(IMessenger messenger, ModulesInfo modules, BuildResourcesInfo buildResources) : base(messenger)
    {
        _modules = modules;
        _buildResources = buildResources;

        _modules.Modules.CollectionChanged += OnModulesCollectionChanged;

        Messenger.Register<BuildResourcesGridModel, PropertyChangedMessage<IEnumerable<IEquipment>>, string>(this, $"{nameof(ModulesGridItem)}.{nameof(ModulesGridItem.Equipments)}", static (r, m) => r.OnModuleEquipmentChanged((m.Sender as ModulesGridItem)!, m.OldValue));
        Messenger.RegisterPropertyChangedMessage(this, static (ModulesGridItem x) => x.SelectedMethod, static (r, m) => r.OnModuleSelectedMethodChanged((m.Sender as ModulesGridItem)!, m.OldValue.Method));
        Messenger.RegisterPropertyChangedMessage(this, static (ModulesGridItem x) => x.ModuleCount,    static (r, m) => r.OnModuleCountChanged(m));
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        Resources.Clear();
        _modules.Modules.CollectionChanged -= OnModulesCollectionChanged;

        Messenger.UnregisterAll(this);
    }


    /// <summary>
    /// 価格割合一括設定
    /// </summary>
    /// <param name="value">設定値</param>
    public void SetUnitPricePercent(long value)
    {
        foreach (var resource in Resources)
        {
            resource.SetUnitPricePercent(value);
        }
    }


    /// <summary>
    /// 購入しない一括設定
    /// </summary>
    /// <param name="value">設定値</param>
    /// <param name="onlySelectedItem">選択中の項目のみ設定するか</param>
    public void SetNoBuy(bool value, bool onlySelectedItem)
    {
        foreach (var resource in Resources.Where(x => x.IsSelected || !onlySelectedItem))
        {
            resource.NoBuy = value;
        }
    }


    /// <summary>
    /// モジュール一覧変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnModulesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            OnModulesAdded(e.NewItems.Cast<ModulesGridItem>());
        }

        if (e.OldItems is not null)
        {
            OnModulesRemoved(e.OldItems.Cast<ModulesGridItem>());
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            // 前回値保存
            using var optBakDict = Resources.ToPooledDictionary(x => x.Ware.ID);

            Resources.Clear();

            if (_modules.Modules.Any())
            {
                var resources = AggregateModules(_modules.Modules);

                // 可能なら前回値復元して製品一覧に追加
                var addItems = resources.Select(
                    x => optBakDict.TryGetValue(x.WareID, out var oldRes) ? 
                        new BuildResourcesGridItem(Messenger, x.WareID, x.Amount, oldRes.UnitPrice) { EditStatus = oldRes.EditStatus } :
                        new BuildResourcesGridItem(Messenger, x.WareID, x.Amount) { EditStatus = EditStatus.Edited }
                );


                Resources.AddRange(addItems);
            }
        }
    }


    /// <summary>
    /// モジュール数変更時に建造に必要なリソースを更新
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void OnModuleCountChanged(PropertyChangedMessage<long> message)
    {
        if (message.Sender is not ModulesGridItem item)
        {
            throw new InvalidOperationException();
        }

        (IWare Ware, string Method, long Count)[] wares =
        [
            (item.Module, item.SelectedMethod.Method, 1)
        ];


        // モジュールの建造に必要なリソースを集計
        // モジュールの装備一覧(装備IDごとに集計)
        var equipments = item
            .Equipments.AllEquipments
            .Select(x => (Ware: x, Count: 1))
            .GroupBy(x => x)
            .Select(x => (x.Key.Ware as IWare, Method: "default", Count: x.LongCount()));

        IEnumerable<CalcResult> resources = _calculator.CalcResource(wares.Concat(equipments));

        foreach (var resource in resources)
        {
            var itm = Resources.FirstOrDefault(x => x.Ware.ID == resource.WareID);
            if (itm is not null)
            {
                itm.Amount += resource.Amount * (item.ModuleCount - message.OldValue);
            }
        }
    }


    /// <summary>
    /// モジュールの建造方式変更時に必要なリソースを更新
    /// </summary>
    /// <param name="module"></param>
    /// <param name="prevBuildMethod"></param>
    private void OnModuleSelectedMethodChanged(ModulesGridItem module, string prevBuildMethod)
    {
        (IWare Ware, string Method, long ModuleCount)[] modules =
        [
            (module.Module, prevBuildMethod, -1),                   // 変更前のため -1
            (module.Module, module.SelectedMethod.Method, 1)        // 変更後のため +1
        ];

        using var addTarget = new PooledList<BuildResourcesGridItem>();
        foreach (var kvp in _calculator.CalcResource(modules))
        {
            var item = Resources.FirstOrDefault(x => x.Ware.ID == kvp.WareID);
            if (item is not null)
            {
                // 既にウェアが一覧にある場合
                item.Amount += kvp.Amount * module.ModuleCount;
            }
            else
            {
                // ウェアが一覧にない場合
                addTarget.Add(new BuildResourcesGridItem(Messenger, kvp.WareID, kvp.Amount * module.ModuleCount) { EditStatus = EditStatus.Edited });
            }
        }

        Resources.AddRange(addTarget);
        Resources.RemoveAll(x => x.Amount == 0);
    }


    /// <summary>
    /// モジュールの装備変更時に建造に必要なリソースを更新
    /// </summary>
    /// <param name="module">変更対象モジュール</param>
    /// <param name="prevEquipments">前回装備</param>
    private void OnModuleEquipmentChanged(ModulesGridItem module, IEnumerable<IEquipment> prevEquipments)
    {
        // 新しい装備一覧
        var newEquipments = module.Equipments.AllEquipments
            .GroupBy(x => x)
            .Select(x => (x.Key as IWare, "default", (long)x.Count()));

        // 古い装備一覧
        var oldEquipments = prevEquipments
            .GroupBy(x => x)
            .Select(x => (x.Key as IWare, "default", -(long)x.Count()));

        // リソース集計
        using var addTarget = new PooledList<BuildResourcesGridItem>();
        foreach (var resource in _calculator.CalcResource(newEquipments.Concat(oldEquipments)))
        {
            var item = Resources.FirstOrDefault(x => x.Ware.ID == resource.WareID);
            if (item is not null)
            {
                // 既にウェアが一覧にある場合
                item.Amount += resource.Amount * module.ModuleCount;
            }
            else
            {
                // ウェアが一覧にない場合
                addTarget.Add(new BuildResourcesGridItem(Messenger, resource.WareID, resource.Amount * module.ModuleCount) { EditStatus = EditStatus.Edited });
            }
        }

        Resources.AddRange(addTarget);
        Resources.RemoveAll(x => x.Amount == 0);
    }


    /// <summary>
    /// モジュールが追加された場合
    /// </summary>
    /// <param name="modules">追加されたモジュール</param>
    private void OnModulesAdded(IEnumerable<ModulesGridItem> modules)
    {
        using var addTarget = new PooledList<BuildResourcesGridItem>();
        using var resources = Resources.ToPooledDictionary(x => x.Ware.ID, x => x);

        foreach (var kvp in AggregateModules(modules))
        {
            if (resources.TryGetValue(kvp.WareID, out var item))
            {
                // 既にウェアが一覧にある場合
                item.Amount += kvp.Amount;
            }
            else
            {
                // ウェアが一覧にない場合
                item = new BuildResourcesGridItem(Messenger, kvp.WareID, kvp.Amount) { EditStatus = EditStatus.Edited };
                resources.Add(kvp.WareID, item);
                addTarget.Add(item);
            }
        }

        Resources.AddRange(addTarget);
    }


    /// <summary>
    /// モジュールが削除された場合
    /// </summary>
    /// <param name="modules">削除されたモジュール</param>
    private void OnModulesRemoved(IEnumerable<ModulesGridItem> modules)
    {
        using var resources = Resources.ToPooledDictionary(x => x.Ware.ID, x => x);

        foreach (var result in AggregateModules(modules))
        {
            resources[result.WareID].Amount -= result.Amount;
        }

        Resources.RemoveAll(x => x.Amount == 0);
    }


    /// <summary>
    /// 建造に必要な情報を集計
    /// </summary>
    /// <param name="modules">集計対象モジュール</param>
    /// <returns>集計結果</returns>
    private IEnumerable<CalcResult> AggregateModules(IEnumerable<ModulesGridItem> modules)
    {
        // 同一ウェアを事前に集計するための、ウェアと建造方式をキーにした数量のディクショナリ
        using var tmp = new PooledDictionary<(IWare Ware, string Method), long>(256);

        foreach (var module in modules)
        {
            if (!tmp.TryAdd((module.Module, module.SelectedMethod.Method), module.ModuleCount))
            {
                tmp[(module.Module, module.SelectedMethod.Method)] += module.ModuleCount;
            }

            foreach (var equipment in module.Equipments.AllEquipments)
            {
                if (!tmp.TryAdd((equipment, "default"), 1))
                {
                    tmp[(equipment, "default")] += 1;
                }
            }
        }

        foreach (var resource in _calculator.CalcResource(tmp.Select(x => (x.Key.Ware, x.Key.Method, x.Value))))
        {
            yield return resource;
        }
    }
}

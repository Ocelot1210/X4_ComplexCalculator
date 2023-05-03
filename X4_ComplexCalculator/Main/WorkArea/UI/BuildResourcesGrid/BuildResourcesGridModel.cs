using Collections.Pooled;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.BuildResources;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;

namespace X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;

/// <summary>
/// 建造に必要なリソースを表示するDataGridView用Model
/// </summary>
class BuildResourcesGridModel : IDisposable
{
    #region メンバ
    /// <summary>
    /// モジュール一覧
    /// </summary>
    private readonly IModulesInfo _modules;


    /// <summary>
    /// 建造リソース情報
    /// </summary>
    private readonly IBuildResourcesInfo _buildResources;


    /// <summary>
    /// 前回値オプション保存用
    /// </summary>
    private readonly Dictionary<string, BuildResourcesGridItem> _optionsBakDict = new();


    /// <summary>
    /// 建造リソース計算用
    /// </summary>
    private readonly BuildResourceCalculator _calculator = BuildResourceCalculator.Instance;
    #endregion


    #region プロパティ
    /// <summary>
    /// 建造に必要なリソース
    /// </summary>
    public ObservablePropertyChangedCollection<BuildResourcesGridItem> Resources => _buildResources.BuildResources;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="modules">モジュール一覧情報</param>
    /// <param name="buildResources">建造リソース情報</param>
    public BuildResourcesGridModel(IModulesInfo modules, IBuildResourcesInfo buildResources)
    {
        _modules = modules;
        _buildResources = buildResources;

        _modules.Modules.CollectionChanged += OnModulesCollectionChanged;
        _modules.Modules.CollectionPropertyChanged += OnModulesPropertyChanged;
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        Resources.Clear();
        _modules.Modules.CollectionChanged -= OnModulesCollectionChanged;
        _modules.Modules.CollectionPropertyChanged -= OnModulesPropertyChanged;
    }


    /// <summary>
    /// モジュールのプロパティ変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private void OnModulesPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (sender is not ModulesGridItem module)
        {
            return;
        }

        switch (e.PropertyName)
        {
            // モジュール数変更の場合
            case nameof(ModulesGridItem.ModuleCount):
                {
                    if (e is PropertyChangedExtendedEventArgs<long> ev)
                    {
                        OnModuleCountChanged(module, ev.OldValue);
                    }
                }

                break;

            // 装備変更の場合
            case nameof(ModulesGridItem.Equipments):
                {
                    if (e is PropertyChangedExtendedEventArgs<IEnumerable<string>> ev)
                    {
                        OnModuleEquipmentChanged(module, ev.OldValue);
                    }
                }

                break;

            // 建造方式変更の場合
            case nameof(ModulesGridItem.SelectedMethod):
                {
                    if (e is PropertyChangedExtendedEventArgs<IWareProduction> ev)
                    {
                        OnModuleSelectedMethodChanged(module, ev.OldValue.Method);
                    }
                }
                break;

            default:
                break;
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
            foreach (var resource in Resources)
            {
                if (!_optionsBakDict.TryAdd(resource.Ware.ID, resource))
                {
                    _optionsBakDict[resource.Ware.ID] = resource;
                }
            }

            Resources.Clear();

            if (_modules.Modules.Any())
            {
                var resources = AggregateModules(_modules.Modules);

                // 可能なら前回値復元して製品一覧に追加
                var addItems = resources.Select(
                    x =>
                    {
                        if (_optionsBakDict.TryGetValue(x.WareID, out var oldRes))
                        {
                            return new BuildResourcesGridItem(x.WareID, x.Amount, oldRes.UnitPrice) { EditStatus = oldRes.EditStatus };
                        }

                        return new BuildResourcesGridItem(x.WareID, x.Amount) { EditStatus = EditStatus.Edited };
                    });

                Resources.AddRange(addItems);
                _optionsBakDict.Clear();
            }
        }
    }


    /// <summary>
    /// モジュール数変更時に建造に必要なリソースを更新
    /// </summary>
    /// <param name="module">変更対象モジュール</param>
    /// <param name="prevModuleCount">モジュール数前回値</param>
    private void OnModuleCountChanged(ModulesGridItem module, long prevModuleCount)
    {
        (IWare Ware, string Method, long Count)[] wares = 
        {
            (module.Module, module.SelectedMethod.Method, 1)
        };


        // モジュールの建造に必要なリソースを集計
        // モジュールの装備一覧(装備IDごとに集計)
        var equipments = module
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
                itm.Amount += resource.Amount * (module.ModuleCount - prevModuleCount);
            }
        }
    }


    /// <summary>
    /// モジュールの建造方式変更時に必要なリソースを更新
    /// </summary>
    /// <param name="module"></param>
    /// <param name="buildMethod"></param>
    private void OnModuleSelectedMethodChanged(ModulesGridItem module, string buildMethod)
    {
        (IWare Ware, string Method, long ModuleCount)[] modules =
        {
            (module.Module, buildMethod, -1),                   // 変更前のため -1
            (module.Module, module.SelectedMethod.Method, 1)    // 変更後のため +1
        };

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
                addTarget.Add(new BuildResourcesGridItem(kvp.WareID, kvp.Amount * module.ModuleCount) { EditStatus = EditStatus.Edited });
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
    private void OnModuleEquipmentChanged(ModulesGridItem module, IEnumerable<string> prevEquipments)
    {
        // 新しい装備一覧
        var newEquipments = module.Equipments.AllEquipments
            .GroupBy(x => x)
            .Select(x => (x.Key as IWare, "default", (long)x.Count()));

        // 古い装備一覧
        var oldEquipments = prevEquipments
            .GroupBy(x => x)
            .Select(x => (X4Database.Instance.Ware.Get(x.Key), "default", -(long)x.Count()));

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
                addTarget.Add(new BuildResourcesGridItem(resource.WareID, resource.Amount * module.ModuleCount) { EditStatus = EditStatus.Edited });
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
        foreach (var kvp in AggregateModules(modules))
        {
            var item = Resources.FirstOrDefault(x => x.Ware.ID == kvp.WareID);
            if (item is not null)
            {
                // 既にウェアが一覧にある場合
                item.Amount += kvp.Amount;
            }
            else
            {
                // ウェアが一覧にない場合
                addTarget.Add(new BuildResourcesGridItem(kvp.WareID, kvp.Amount) { EditStatus = EditStatus.Edited });
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
        IEnumerable<CalcResult> resources = AggregateModules(modules);

        foreach (var kvp in resources)
        {
            var itm = Resources.FirstOrDefault(x => x.Ware.ID == kvp.WareID);
            if (itm is not null)
            {
                itm.Amount -= kvp.Amount;
            }
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
        // モジュール一覧
        var moduleList = modules.Select(x => (x.Module as IWare, x.SelectedMethod.Method, x.ModuleCount));

        var equipments = modules
            .SelectMany(x => x.Equipments.AllEquipments.Select(y => (Ware: y as IWare, Count: x.ModuleCount)))
            .GroupBy(x => x.Ware)
            .Select(x => (Ware: x.Key, Method: "default", Count: x.Sum(y => y.Count)));


        return _calculator.CalcResource(moduleList.Concat(equipments));
    }
}

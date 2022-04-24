using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Storages;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StoragesGrid;

/// <summary>
/// 保管庫一覧表示用DataGridViewのModel
/// </summary>
class StoragesGridModel : IDisposable
{
    #region メンバ
    /// <summary>
    /// モジュール一覧情報
    /// </summary>
    private readonly IModulesInfo _Modules;


    /// <summary>
    /// 保管庫一覧情報
    /// </summary>
    private readonly IStoragesInfo _Storages;
    #endregion


    #region プロパティ
    /// <summary>
    /// ストレージ一覧
    /// </summary>
    public ObservablePropertyChangedCollection<StoragesGridItem> Storages => _Storages.Storages;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="modules">モジュール一覧</param>
    /// <param name="storages">保管庫一覧</param>
    public StoragesGridModel(IModulesInfo modules, IStoragesInfo storages)
    {
        _Modules = modules;
        _Storages = storages;
        _Modules.Modules.CollectionChangedAsync += OnModulesChanged;
        _Modules.Modules.CollectionPropertyChangedAsync += OnModulePropertyChanged;
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _Modules.Modules.CollectionChangedAsync -= OnModulesChanged;
        _Modules.Modules.CollectionPropertyChangedAsync -= OnModulePropertyChanged;
    }


    /// <summary>
    /// モジュールのプロパティ変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private async Task OnModulePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // モジュール数変更時のみ処理
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
        
        // 保管モジュールの場合のみ更新
        if (0 < module.Module.Storage.Amount && module.Module.Storage.Types.Any())
        {
            if (e is not PropertyChangedExtendedEventArgs<long> ev)
            {
                await Task.CompletedTask;
                return;
            }
            OnModuleCountChanged(module, ev.OldValue);
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
            OnModulesAdded(e.NewItems.Cast<ModulesGridItem>());
        }

        if (e.OldItems is not null)
        {
            OnModulesRemoved(e.OldItems.Cast<ModulesGridItem>());
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            Storages.Clear();
            OnModulesAdded(_Modules.Modules);
        }

        await Task.CompletedTask;
    }


    /// <summary>
    /// モジュールが追加された時
    /// </summary>
    /// <param name="modules"></param>
    private void OnModulesAdded(IEnumerable<ModulesGridItem> modules)
    {
        var storageModules = AggregateStorage(modules);

        var addTarget = new List<StoragesGridItem>();

        foreach (var kvp in storageModules)
        {
            // 一致するレコードを探す
            var itm = Storages.FirstOrDefault(x => x.TransportType.Equals(kvp.Key));
            if (itm is not null)
            {
                // 既にレコードがある場合
                itm.AddDetails(kvp.Value);
            }
            else
            {
                // 初回追加の場合
                addTarget.Add(new StoragesGridItem(kvp.Key, kvp.Value));
            }
        }

        Storages.AddRange(addTarget);
    }


    /// <summary>
    /// モジュールが削除された時
    /// </summary>
    /// <param name="modules"></param>
    private void OnModulesRemoved(IEnumerable<ModulesGridItem> modules)
    {
        var storageModules = AggregateStorage(modules);

        foreach (var kvp in storageModules)
        {
            // 一致するレコードを探す
            var itm = Storages.FirstOrDefault(x => x.TransportType.Equals(kvp.Key));
            if (itm is not null)
            {
                itm.RemoveDetails(kvp.Value);
            }
        }

        // 空のレコードを削除
        Storages.RemoveAll(x => x.Capacity == 0);
    }


    /// <summary>
    /// モジュール数変更時
    /// </summary>
    /// <param name="module">変更があったモジュール</param>
    /// <param name="prevModuleCount">前回値モジュール数</param>
    private void OnModuleCountChanged(ModulesGridItem module, long prevModuleCount)
    {
        ModulesGridItem[] modules = { module };

        var storageModules = AggregateStorage(modules);

        foreach (var kvp in storageModules)
        {
            // 変更対象のモジュールを検索
            Storages.FirstOrDefault(x => x.TransportType.Equals(kvp.Key))?.SetDetails(kvp.Value, prevModuleCount);
        }
    }



    /// <summary>
    /// モジュール情報を保管庫種別単位に集計
    /// </summary>
    /// <param name="modules">集計対象</param>
    /// <returns>集計結果</returns>
    private IReadOnlyDictionary<ITransportType, IReadOnlyList<StorageDetailsListItem>> AggregateStorage(IEnumerable<ModulesGridItem> modules)
    {
        return modules
            .Where(x => 0 < x.Module.Storage.Amount && x.Module.Storage.Types.Any())
            .GroupBy(x => x.Module.ID)
            .Select(x => (x.First().Module, Count: x.Sum(y => y.ModuleCount)))
            .SelectMany(x => x.Module.Storage.Types.Select(y => new StorageDetailsListItem(x.Module, x.Count, y)))
            .GroupBy(x => x.TransportType)
            .ToDictionary(x => x.Key, x => x.ToArray() as IReadOnlyList<StorageDetailsListItem>);
    }
}

using Collections.Pooled;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collections;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Storages;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StoragesGrid;

/// <summary>
/// 保管庫一覧表示用DataGridViewのModel
/// </summary>
sealed partial class StoragesGridModel : ObservableRecipientEx, IDisposable
{
    #region メンバ
    /// <summary>
    /// モジュール一覧情報
    /// </summary>
    private readonly IModulesInfo _modules;


    /// <summary>
    /// 保管庫一覧情報
    /// </summary>
    private readonly IStoragesInfo _storages;
    #endregion


    #region プロパティ
    /// <summary>
    /// ストレージ一覧
    /// </summary>
    public ObservablePropertyChangedCollection<StoragesGridItem> Storages => _storages.Storages;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="modules">モジュール一覧</param>
    /// <param name="storages">保管庫一覧</param>
    public StoragesGridModel(IMessenger messenger, IModulesInfo modules, IStoragesInfo storages) : base(messenger, true)
    {
        _modules = modules;
        _storages = storages;
        _modules.Modules.CollectionChanged += OnModulesChanged;

        Messenger.RegisterPropertyChangedMessage(this, static (ModulesGridItem x) => x.ModuleCount, OnModuleCountChanged);
    }


    /// <summary>
    /// モジュール数変更時
    /// </summary>
    private void OnModuleCountChanged(StoragesGridModel recipient, PropertyChangedMessage<long> message)
    {
        if (message.Sender is not ModulesGridItem module)
        {
            return;
        }

        // 保管モジュールの場合のみ更新
        if (0 < module.Module.Storage.Amount && module.Module.Storage.Types.Any())
        {
            ModulesGridItem[] modules = [module];

            var storageModules = AggregateStorage(modules);

            foreach (var kvp in storageModules)
            {
                // 変更対象のモジュールを検索
                Storages.FirstOrDefault(x => x.TransportType.Equals(kvp.Key))?.SetDetails(kvp.Value, message.OldValue);
            }
        }
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _modules.Modules.CollectionChanged -= OnModulesChanged;
        Messenger.UnregisterPropertyChangedMessage(this, static (ModulesGridItem x) => x.ModuleCount);
    }


    /// <summary>
    /// 選択されたアイテムの展開/折りたたみ状態を設定
    /// </summary>
    /// <param name="value">設定値</param>
    public void SetExpanded(bool value)
    {
        foreach (var item in Storages.Where(x => x.IsSelected))
        {
            item.IsExpanded = value;
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
            OnModulesAdded(e.NewItems.Cast<ModulesGridItem>());
        }

        if (e.OldItems is not null)
        {
            OnModulesRemoved(e.OldItems.Cast<ModulesGridItem>());
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            Storages.Clear();
            OnModulesAdded(_modules.Modules);
        }
    }


    /// <summary>
    /// モジュールが追加された時
    /// </summary>
    /// <param name="modules"></param>
    private void OnModulesAdded(IEnumerable<ModulesGridItem> modules)
    {
        var storageModules = AggregateStorage(modules);

        using var addTarget = new PooledList<StoragesGridItem>();

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
            itm?.RemoveDetails(kvp.Value);
        }

        // 空のレコードを削除
        Storages.RemoveAll(x => x.Capacity == 0);
    }


    /// <summary>
    /// モジュール情報を保管庫種別単位に集計
    /// </summary>
    /// <param name="modules">集計対象</param>
    /// <returns>集計結果</returns>
    private static IReadOnlyDictionary<ITransportType, IReadOnlyList<StorageDetailsListItem>> AggregateStorage(IEnumerable<ModulesGridItem> modules)
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

using Collections.Pooled;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.StoragesGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Products;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StorageAssign;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Storages;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;

/// <summary>
/// 保管庫割当用Model
/// </summary>
sealed partial class StorageAssignModel : ObservableRecipientEx, IDisposable
{
    #region メンバ
    /// <summary>
    /// 製品一覧情報
    /// </summary>
    private readonly ProductsInfo _products;


    /// <summary>
    /// 保管庫一覧情報
    /// </summary>
    private readonly StoragesInfo _storages;


    /// <summary>
    /// 保管庫割当情報
    /// </summary>
    private readonly StorageAssignInfo _storageAssignInfo;


    /// <summary>
    /// 保管庫容量情報
    /// </summary>
    private readonly Dictionary<string, StorageCapacityInfo> _capacityDict;


    /// <summary>
    /// 前回値保存用
    /// </summary>
    private readonly Dictionary<string, StorageAssignGridItem> _optionsBakDict = new();
    #endregion


    #region プロパティ
    /// <summary>
    /// 保管庫状態計算用の指定時間
    /// </summary>
    public ObservableCollection<StorageAssignGridItem> StorageAssignGridItems => _storageAssignInfo.StorageAssign;


    /// <summary>
    /// 指定時間
    /// </summary>
    [ObservableProperty]
    public partial long Hour { get; set; } = 1;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messenger">メッセージ通知用</param>
    /// <param name="products">製品一覧</param>
    /// <param name="storages">保管庫情報</param>
    /// <param name="storageAssignInfo">保管庫割当情報</param>
    public StorageAssignModel(IMessenger messenger, ProductsInfo products, StoragesInfo storages, StorageAssignInfo storageAssignInfo) : base(messenger, true)
    {
        _products = products;
        _products.Products.CollectionChanged += Products_CollectionChanged;

        _storages = storages;
        _storages.Storages.CollectionChanged += Storages_CollectionChanged;

        _storageAssignInfo = storageAssignInfo;

        _capacityDict = X4Database.Instance.TransportType.GetAll()
            .ToDictionary(x => x.TransportTypeID, x => new StorageCapacityInfo(Messenger));

        foreach (var storage in _storages.Storages)
        {
            _capacityDict[storage.TransportType.TransportTypeID].TotalCapacity = storage.Capacity;
        }

        Messenger.RegisterPropertyChangedMessage(this, static (ProductsGridItem x) => x.Count, static (r, m) => r.OnProductsCountChanged(m));
        Messenger.RegisterPropertyChangedMessage(this, static (StoragesGridItem x) => x.Capacity, static (r, m) => r.OnStorageCapacityChanged(m));
    }


    /// <summary>
    /// 時間変更時
    /// </summary>
    /// <param name="value">変更値</param>
    partial void OnHourChanged(long value)
    {
        foreach (var item in _storageAssignInfo.StorageAssign)
        {
            item.Hour = value;
        }
    }


    /// <summary>
    /// 製品一覧の生産量変更時
    /// </summary>
    private void OnProductsCountChanged(PropertyChangedMessage<long> message)
    {
        if (message.Sender is not ProductsGridItem product)
        {
            return;
        }

        var assign = _storageAssignInfo.StorageAssign.FirstOrDefault(x => x.WareID == product.Ware.ID);
        if (assign is not null)
        {
            assign.ProductPerHour = product.Count;
        }
    }


    /// <summary>
    /// 保管庫の容量変更時
    /// </summary>
    private void OnStorageCapacityChanged(PropertyChangedMessage<long> message)
    {
        if(message.Sender is not StoragesGridItem storage)
        {
            return;
        }

        // 同一カーゴ種別の総容量設定
        _capacityDict[storage.TransportType.TransportTypeID].TotalCapacity = storage.Capacity;
    }


    /// <summary>
    /// 保管庫一覧変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Storages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (var storage in e.NewItems.Cast<StoragesGridItem>())
            {
                _capacityDict[storage.TransportType.TransportTypeID].TotalCapacity = storage.Capacity;
            }
        }

        if (e.OldItems is not null)
        {
            foreach (var storage in e.OldItems.Cast<StoragesGridItem>())
            {
                _capacityDict[storage.TransportType.TransportTypeID].TotalCapacity = 0;
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var capacityInfo in _capacityDict.Values)
            {
                capacityInfo.TotalCapacity = 0;
            }
        }
    }


    /// <summary>
    /// 製品一覧変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Products_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            OnProductsAdded(e.NewItems.Cast<ProductsGridItem>());
        }

        if (e.OldItems is not null)
        {
            OnProductsRemoved(e.OldItems.Cast<ProductsGridItem>());
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            // 前回値保存
            foreach (var itm in _storageAssignInfo.StorageAssign)
            {
                _optionsBakDict.Add(itm.WareID, itm);
            }

            _storageAssignInfo.StorageAssign.Clear();
        }
    }


    /// <summary>
    /// 製品追加時
    /// </summary>
    /// <param name="products"></param>
    private void OnProductsAdded(IEnumerable<ProductsGridItem> products)
    {
        // 前回値がある場合
        if (_storageAssignInfo.StorageAssign.Count == 0 && 0 < _optionsBakDict.Count)
        {
            _storageAssignInfo.StorageAssign.AddRange(products.Select(prod =>
            {
                var ret = new StorageAssignGridItem(Messenger, prod.Ware, _capacityDict[prod.Ware.TransportType.TransportTypeID], prod.Count, Hour) { EditStatus = EditStatus.Edited };
                if (_optionsBakDict.TryGetValue(ret.WareID, out var oldAssign))
                {
                    ret.AllocCount = oldAssign.AllocCount;
                    ret.EditStatus = oldAssign.EditStatus;
                }

                return ret;
            }));

            _optionsBakDict.Clear();
        }
        else
        {
            _storageAssignInfo.StorageAssign.AddRange(products.Select(prod =>
            {
                return new StorageAssignGridItem(Messenger, prod.Ware, _capacityDict[prod.Ware.TransportType.TransportTypeID], prod.Count, Hour) { EditStatus = EditStatus.Edited };
            }));
        }
    }


    /// <summary>
    /// 製品削除時
    /// </summary>
    /// <param name="products"></param>
    private void OnProductsRemoved(IEnumerable<ProductsGridItem> products)
    {
        // 解放される容量のディクショナリ
        var releasedCapacity = new Dictionary<string, long>();

        // 削除対象のウェア
        using var removeWares = new PooledList<string>();


        foreach (var prod in products)
        {
            // 開放される容量のディクショナリにキーがなければ追加
            if (!releasedCapacity.ContainsKey(prod.Ware.TransportType.TransportTypeID))
            {
                releasedCapacity.Add(prod.Ware.TransportType.TransportTypeID, 0);
            }

            var assign = _storageAssignInfo.StorageAssign.First(x => x.WareID == prod.Ware.ID);

            releasedCapacity[assign.TransportTypeID] += assign.AllocCapacity;
            removeWares.Add(assign.WareID);
            assign.Dispose();
        }

        // 削除対象を削除する
        _storageAssignInfo.StorageAssign.RemoveAll(x => removeWares.Contains(x.WareID));

        // 容量開放
        foreach (var kvp in releasedCapacity)
        {
            _capacityDict[kvp.Key].UsedCapacity -= kvp.Value;
        }
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _products.Products.CollectionChanged -= Products_CollectionChanged;
        _storages.Storages.CollectionChanged -= Storages_CollectionChanged;
    }
}

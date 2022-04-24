using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
class StorageAssignModel : IDisposable
{
    #region メンバ
    /// <summary>
    /// 保管庫状態計算用の指定時間
    /// </summary>
    private long _Hour = 1;


    /// <summary>
    /// 製品一覧情報
    /// </summary>
    private readonly IProductsInfo _Products;


    /// <summary>
    /// 保管庫一覧情報
    /// </summary>
    private readonly IStoragesInfo _Storages;


    /// <summary>
    /// 保管庫割当情報
    /// </summary>
    private readonly IStorageAssignInfo _StorageAssignInfo;


    /// <summary>
    /// 保管庫容量情報
    /// </summary>
    private readonly Dictionary<string, StorageCapacityInfo> _CapacityDict;


    /// <summary>
    /// 前回値保存用
    /// </summary>
    private readonly Dictionary<string, StorageAssignGridItem> _OptionsBakDict = new();
    #endregion


    #region プロパティ
    /// <summary>
    /// 保管庫状態計算用の指定時間
    /// </summary>
    public ObservableCollection<StorageAssignGridItem> StorageAssignGridItems => _StorageAssignInfo.StorageAssign;


    /// <summary>
    /// 指定時間
    /// </summary>
    public long Hour
    {
        get => _Hour;
        set
        {
            if (_Hour != value)
            {
                _Hour = value;
                foreach (var item in _StorageAssignInfo.StorageAssign)
                {
                    item.Hour = Hour;
                }
            }
        }
    }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="products">製品一覧</param>
    /// <param name="storages">保管庫情報</param>
    /// <param name="storageAssignInfo">保管庫割当情報</param>
    public StorageAssignModel(IProductsInfo products, IStoragesInfo storages, IStorageAssignInfo storageAssignInfo)
    {
        _Products = products;
        _Products.Products.CollectionChanged += Products_CollectionChanged;
        _Products.Products.CollectionPropertyChanged += Products_CollectionPropertyChanged;

        _Storages = storages;
        _Storages.Storages.CollectionChanged += Storages_CollectionChanged;
        _Storages.Storages.CollectionPropertyChanged += Storages_CollectionPropertyChanged;

        _StorageAssignInfo = storageAssignInfo;

        _CapacityDict = X4Database.Instance.TransportType.GetAll()
            .ToDictionary(x => x.TransportTypeID, x => new StorageCapacityInfo());

        foreach (var storage in _Storages.Storages)
        {
            _CapacityDict[storage.TransportType.TransportTypeID].TotalCapacity = storage.Capacity;
        }
    }


    /// <summary>
    /// 保管庫のプロパティ変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Storages_CollectionPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (sender is not StoragesGridItem storage)
        {
            return;
        }

        switch (e.PropertyName)
        {
            case nameof(StoragesGridItem.Capacity):
                // 同一カーゴ種別の総容量設定
                _CapacityDict[storage.TransportType.TransportTypeID].TotalCapacity = storage.Capacity;
                break;

            default:
                break;
        }
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
                _CapacityDict[storage.TransportType.TransportTypeID].TotalCapacity = storage.Capacity;
            }
        }

        if (e.OldItems is not null)
        {
            foreach (var storage in e.OldItems.Cast<StoragesGridItem>())
            {
                _CapacityDict[storage.TransportType.TransportTypeID].TotalCapacity = 0;
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var capacityInfo in _CapacityDict.Values)
            {
                capacityInfo.TotalCapacity = 0;
            }
        }
    }


    /// <summary>
    /// 製品一覧のプロパティ変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Products_CollectionPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ProductsGridItem.Count))
        {
            return;
        }

        if (sender is not ProductsGridItem product)
        {
            return;
        }

        var assign = _StorageAssignInfo.StorageAssign.FirstOrDefault(x => x.WareID == product.Ware.ID);
        if (assign is not null)
        {
            assign.ProductPerHour = product.Count;
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
            foreach (var itm in _StorageAssignInfo.StorageAssign)
            {
                _OptionsBakDict.Add(itm.WareID, itm);
            }

            _StorageAssignInfo.StorageAssign.Clear();
        }
    }


    /// <summary>
    /// 製品追加時
    /// </summary>
    /// <param name="products"></param>
    private void OnProductsAdded(IEnumerable<ProductsGridItem> products)
    {
        // 前回値がある場合
        if (_StorageAssignInfo.StorageAssign.Count == 0 && 0 < _OptionsBakDict.Count)
        {
            _StorageAssignInfo.StorageAssign.AddRange(products.Select(prod =>
            {
                var ret = new StorageAssignGridItem(prod.Ware, _CapacityDict[prod.Ware.TransportType.TransportTypeID], prod.Count, Hour) { EditStatus = EditStatus.Edited };
                if (_OptionsBakDict.TryGetValue(ret.WareID, out var oldAssign))
                {
                    ret.AllocCount = oldAssign.AllocCount;
                    ret.EditStatus = oldAssign.EditStatus;
                }

                return ret;
            }));

            _OptionsBakDict.Clear();
        }
        else
        {
            _StorageAssignInfo.StorageAssign.AddRange(products.Select(prod =>
            {
                return new StorageAssignGridItem(prod.Ware, _CapacityDict[prod.Ware.TransportType.TransportTypeID], prod.Count, Hour) { EditStatus = EditStatus.Edited };
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
        var removeWares = new List<string>();


        foreach (var prod in products)
        {
            // 開放される容量のディクショナリにキーがなければ追加
            if (!releasedCapacity.ContainsKey(prod.Ware.TransportType.TransportTypeID))
            {
                releasedCapacity.Add(prod.Ware.TransportType.TransportTypeID, 0);
            }

            var assign = _StorageAssignInfo.StorageAssign.First(x => x.WareID == prod.Ware.ID);

            releasedCapacity[assign.TransportTypeID] += assign.AllocCapacity;
            removeWares.Add(assign.WareID);
            assign.Dispose();
        }

        // 削除対象を削除する
        _StorageAssignInfo.StorageAssign.RemoveAll(x => removeWares.Contains(x.WareID));

        // 容量開放
        foreach (var kvp in releasedCapacity)
        {
            _CapacityDict[kvp.Key].UsedCapacity -= kvp.Value;
        }
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _Products.Products.CollectionChanged -= Products_CollectionChanged;
        _Products.Products.CollectionPropertyChanged -= Products_CollectionPropertyChanged;

        _Storages.Storages.CollectionChanged -= Storages_CollectionChanged;
        _Storages.Storages.CollectionPropertyChanged -= Storages_CollectionPropertyChanged;
    }
}

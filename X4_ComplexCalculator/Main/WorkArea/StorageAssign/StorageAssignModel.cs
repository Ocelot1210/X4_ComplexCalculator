using System;
using System.Collections.Generic;
using System.Text;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.StoragesGrid;
using System.Collections.Specialized;
using System.ComponentModel;
using X4_ComplexCalculator.DB;
using System.Linq;

namespace X4_ComplexCalculator.Main.WorkArea.StorageAssign
{
    /// <summary>
    /// 保管庫割当用Model
    /// </summary>
    class StorageAssignModel : IDisposable
    {
        #region メンバ
        private long _Hour = 1;

        /// <summary>
        /// 製品一覧
        /// </summary>
        private ObservablePropertyChangedCollection<ProductsGridItem> _Products;


        /// <summary>
        /// 保管庫一覧
        /// </summary>
        private ObservablePropertyChangedCollection<StoragesGridItem> _Storages;


        /// <summary>
        /// 保管庫容量情報
        /// </summary>
        private Dictionary<string, StorageCapacityInfo> _CapacityDict = new Dictionary<string, StorageCapacityInfo>();
        #endregion

        #region プロパティ
        /// <summary>
        /// 保管庫割当情報
        /// </summary>
        public ObservablePropertyChangedCollection<StorageAssignGridItem> StorageAssignGridItems { get; } = new ObservablePropertyChangedCollection<StorageAssignGridItem>();


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
                    foreach (var item in StorageAssignGridItems)
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
        public StorageAssignModel(ObservablePropertyChangedCollection<ProductsGridItem> products, ObservablePropertyChangedCollection<StoragesGridItem> storages)
        {
            _Products = products;
            _Products.CollectionChanged += Products_CollectionChanged;
            _Products.CollectionPropertyChanged += Products_CollectionPropertyChanged;

            _Storages = storages;
            _Storages.CollectionChanged += Storages_CollectionChanged;
            _Storages.CollectionPropertyChanged += Storages_CollectionPropertyChanged;

            DBConnection.X4DB.ExecQuery("SELECT TransportTypeID FROM TransportType", (dr, args) =>
            {
                _CapacityDict.Add((string)dr["TransportTypeID"], new StorageCapacityInfo());
            });

            foreach (var storage in _Storages)
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
            if (!(sender is StoragesGridItem storage))
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
        private void Storages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var storage in e.NewItems.Cast<StoragesGridItem>())
                {
                    _CapacityDict[storage.TransportType.TransportTypeID].TotalCapacity = storage.Capacity;
                }
            }

            if (e.OldItems != null)
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

            if (!(sender is ProductsGridItem product))
            {
                return;
            }

            var assign = StorageAssignGridItems.Where(x => x.WareID == product.Ware.WareID).First();
            assign.ProductPerHour = product.Count;
        }


        /// <summary>
        /// 製品一覧変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Products_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                OnProductsAdded(e.NewItems.Cast<ProductsGridItem>());
            }

            if (e.OldItems != null)
            {
                OnProductsRemoved(e.NewItems.Cast<ProductsGridItem>());
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                OnProductsAdded(_Products);
            }
        }


        /// <summary>
        /// 製品追加時
        /// </summary>
        /// <param name="products"></param>
        private void OnProductsAdded(IEnumerable<ProductsGridItem> products)
        {
            StorageAssignGridItems.AddRange(products.Select(prod =>
            {
                return new StorageAssignGridItem(prod.Ware, _CapacityDict[prod.Ware.TransportType.TransportTypeID], prod.Count, Hour);
            }));
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

                var assign = StorageAssignGridItems.Where(x => x.WareID == prod.Ware.WareID).First();

                releasedCapacity[assign.TransportTypeID] += assign.AllocCapacity;
                removeWares.Add(assign.WareID);
                assign.Dispose();
            }

            // 削除対象を削除する
            StorageAssignGridItems.RemoveAll(x => removeWares.Contains(x.WareID));

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
            _Products.CollectionChanged -= Products_CollectionChanged;
            _Products.CollectionPropertyChanged -= Products_CollectionPropertyChanged;

            _Storages.CollectionChanged -= Storages_CollectionChanged;
            _Storages.CollectionPropertyChanged -= Storages_CollectionPropertyChanged;
        }
    }
}

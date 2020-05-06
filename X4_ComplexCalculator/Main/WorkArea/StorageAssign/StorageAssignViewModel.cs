using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.StoragesGrid;
using System.ComponentModel;

namespace X4_ComplexCalculator.Main.WorkArea.StorageAssign
{
    /// <summary>
    /// 保管庫割当用ViewModel
    /// </summary>
    class StorageAssignViewModel : INotifyPropertyChangedBace, IDisposable
    {
        #region メンバ
        /// <summary>
        /// 保管庫割当用Model
        /// </summary>
        StorageAssignModel _Model;
        #endregion

        #region プロパティ
        /// <summary>
        /// 保管庫割当情報
        /// </summary>
        public ListCollectionView StorageAssignInfo { get; }


        /// <summary>
        /// 指定時間
        /// </summary>
        public long Hour
        {
            get => _Model.Hour;
            set => _Model.Hour = value;
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="products">製品一覧</param>
        /// <param name="storages">保管庫情報</param>
        public StorageAssignViewModel(ObservablePropertyChangedCollection<ProductsGridItem> products, ObservablePropertyChangedCollection<StoragesGridItem> storages)
        {
            _Model = new StorageAssignModel(products, storages);

            StorageAssignInfo = (ListCollectionView)CollectionViewSource.GetDefaultView(_Model.StorageAssignGridItems);
            StorageAssignInfo.SortDescriptions.Clear();
            StorageAssignInfo.SortDescriptions.Add(new SortDescription(nameof(StorageAssignGridItem.WareName), ListSortDirection.Ascending));

            StorageAssignInfo.GroupDescriptions.Clear();
            StorageAssignInfo.GroupDescriptions.Add(new PropertyGroupDescription(nameof(StorageAssignGridItem.TransportTypeName)));
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _Model.Dispose();
        }
    }
}

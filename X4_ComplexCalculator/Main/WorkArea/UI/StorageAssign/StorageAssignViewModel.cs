using System;
using System.ComponentModel;
using System.Windows.Data;
using Prism.Mvvm;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign
{
    /// <summary>
    /// 保管庫割当用ViewModel
    /// </summary>
    public class StorageAssignViewModel : BindableBase, IDisposable
    {
        #region メンバ
        /// <summary>
        /// 保管庫割当用Model
        /// </summary>
        private readonly StorageAssignModel _Model;
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
        /// <param name="stationData">計算機で使用するステーション情報</param>
        public StorageAssignViewModel(IStationData stationData)
        {
            _Model = new StorageAssignModel(stationData.ProductsInfo, stationData.StoragesInfo, stationData.StorageAssignInfo);

            StorageAssignInfo = (ListCollectionView)CollectionViewSource.GetDefaultView(_Model.StorageAssignGridItems);
            StorageAssignInfo.SortDescriptions.Clear();
            StorageAssignInfo.SortDescriptions.Add(new SortDescription(nameof(StorageAssignGridItem.Tier), ListSortDirection.Ascending));
            StorageAssignInfo.SortDescriptions.Add(new SortDescription(nameof(StorageAssignGridItem.WareName), ListSortDirection.Ascending));

            StorageAssignInfo.GroupDescriptions.Clear();
            StorageAssignInfo.GroupDescriptions.Add(new PropertyGroupDescription(nameof(StorageAssignGridItem.TransportTypeID)));
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose() => _Model.Dispose();
    }
}

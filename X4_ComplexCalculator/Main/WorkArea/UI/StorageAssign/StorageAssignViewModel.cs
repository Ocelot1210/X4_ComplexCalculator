using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Windows.Data;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign
{
    /// <summary>
    /// 保管庫割当用ViewModel
    /// </summary>
    class StorageAssignViewModel : BindableBase, IDisposable
    {
        #region メンバ
        /// <summary>
        /// 保管庫割当用Model
        /// </summary>
        readonly StorageAssignModel _Model;
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
        /// <param name="model">保管庫割当Model</param>
        public StorageAssignViewModel(StorageAssignModel model)
        {
            _Model = model;

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

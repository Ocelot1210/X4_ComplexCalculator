using System.Collections.Generic;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.StorageGrid
{
    /// <summary>
    /// 保管庫一覧表示用DataGridViewの1レコード分
    /// </summary>
    class StoragesGridItem : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// Expanderが展開されているか
        /// </summary>
        private bool _IsExpanded;
        #endregion

        #region プロパティ
        /// <summary>
        /// ウェア種別
        /// </summary>
        public TransportType TransportType { get; private set; }

        /// <summary>
        /// 容量
        /// </summary>
        public long Capacity { get; private set; }

        /// <summary>
        /// 詳細情報(関連モジュール等)
        /// </summary>
        public IReadOnlyCollection<StorageDetailsListItem> Details { get; }


        /// <summary>
        /// Expanderが開いているか
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return _IsExpanded;
            }
            set
            {
                _IsExpanded = value;
                OnPropertyChanged();
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="transportTypeID">カーゴ種別ID</param>
        /// <param name="capacity">容量</param>
        /// <param name="details">詳細情報</param>
        public StoragesGridItem(string transportTypeID, long capacity, IReadOnlyCollection<StorageDetailsListItem> details, bool isExpanded = false)
        {
            TransportType = new TransportType(transportTypeID);
            Capacity = capacity;
            Details = details;
            _IsExpanded = isExpanded;
        }
    }
}

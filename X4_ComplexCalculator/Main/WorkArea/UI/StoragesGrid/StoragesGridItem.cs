using System.Collections.Generic;
using System.Linq;
using Prism.Mvvm;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StoragesGrid
{
    /// <summary>
    /// 保管庫一覧表示用DataGridViewの1レコード分
    /// </summary>
    public class StoragesGridItem : BindableBase, ISelectable
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
        public TransportType TransportType { get; }


        /// <summary>
        /// 容量
        /// </summary>
        public long Capacity => Details.Sum(x => x.TotalCapacity);


        /// <summary>
        /// 詳細情報(関連モジュール等)
        /// </summary>
        public ObservableRangeCollection<StorageDetailsListItem> Details { get; }


        /// <summary>
        /// 選択されているか
        /// </summary>
        public bool IsSelected { get; set; }


        /// <summary>
        /// Expanderが開いているか
        /// </summary>
        public bool IsExpanded
        {
            get => _IsExpanded;
            set => SetProperty(ref _IsExpanded, value);
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="transportType">カーゴ種別</param>
        /// <param name="details">詳細情報</param>
        public StoragesGridItem(TransportType transportType, IEnumerable<StorageDetailsListItem> details)
        {
            TransportType = transportType;
            Details = new ObservableRangeCollection<StorageDetailsListItem>(details);
        }


        /// <summary>
        /// 詳細情報を追加
        /// </summary>
        /// <param name="details"></param>
        public void AddDetails(IEnumerable<StorageDetailsListItem> details)
        {
            var addItems = new List<StorageDetailsListItem>();

            foreach (var item in details)
            {
                var tmp = Details.FirstOrDefault(x => x.ModuleID == item.ModuleID);
                if (tmp is not null)
                {
                    // 既にモジュールがある場合
                    tmp.ModuleCount += item.ModuleCount;
                }
                else
                {
                    // 初回追加の場合
                    addItems.Add(item);
                }
            }

            Details.AddRange(addItems);

            RaisePropertyChanged(nameof(Capacity));
        }


        /// <summary>
        /// 詳細情報を削除
        /// </summary>
        /// <param name="details"></param>
        public void RemoveDetails(IEnumerable<StorageDetailsListItem> details)
        {
            foreach (var item in details)
            {
                var tmp = Details.FirstOrDefault(x => x.ModuleID == item.ModuleID);
                if (tmp is not null)
                {
                    tmp.ModuleCount -= item.ModuleCount;
                }
            }

            Details.RemoveAll(x => x.ModuleCount == 0);

            RaisePropertyChanged(nameof(Capacity));
        }


        /// <summary>
        /// 詳細情報を設定
        /// </summary>
        /// <param name="details">詳細情報</param>
        /// <param name="prevModuleCount">前回モジュール数</param>
        public void SetDetails(IEnumerable<StorageDetailsListItem> details, long prevModuleCount)
        {
            foreach (var item in details)
            {
                var tmp = Details.FirstOrDefault(x => x.ModuleID == item.ModuleID);
                if (tmp is not null)
                {
                    tmp.ModuleCount += item.ModuleCount - prevModuleCount;
                }
            }

            RaisePropertyChanged(nameof(Capacity));
        }
    }
}

using Prism.Mvvm;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign
{
    /// <summary>
    /// 保管庫容量情報
    /// </summary>
    public class StorageCapacityInfo : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 保管庫総容量
        /// </summary>
        private long _TotalCapacity;


        /// <summary>
        /// 保管庫使用容量
        /// </summary>
        private long _UsedCapacity;
        #endregion


        #region プロパティ
        /// <summary>
        /// 保管庫総容量
        /// </summary>
        public long TotalCapacity
        {
            get => _TotalCapacity;
            set
            {
                if (SetProperty(ref _TotalCapacity, value))
                {
                    RaisePropertyChanged(nameof(FreeCapacity));
                }
            }
        }


        /// <summary>
        /// 保管庫使用容量
        /// </summary>
        public long UsedCapacity
        {
            get => _UsedCapacity;
            set
            {
                if (SetProperty(ref _UsedCapacity, value))
                {
                    RaisePropertyChanged(nameof(FreeCapacity));
                }
            }
        }


        /// <summary>
        /// 保管庫空き容量
        /// </summary>
        public long FreeCapacity => TotalCapacity - _UsedCapacity;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="totalCapacity">保管庫総容量</param>
        /// <param name="usedCapacity">保管庫使用容量</param>
        public StorageCapacityInfo(long totalCapacity = 0, long usedCapacity = 0)
        {
            TotalCapacity = totalCapacity;
            UsedCapacity = usedCapacity;
        }
    }
}

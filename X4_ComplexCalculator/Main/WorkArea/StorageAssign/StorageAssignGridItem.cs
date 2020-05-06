using System;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB;
using System.ComponentModel;

namespace X4_ComplexCalculator.Main.WorkArea.StorageAssign
{
    /// <summary>
    /// 保管庫割当用Gridの1レコード分
    /// </summary>
    class StorageAssignGridItem : INotifyPropertyChangedBace, IDisposable
    {
        #region メンバ
        /// <summary>
        /// 指定時間
        /// </summary>
        private long _Hour;

        /// <summary>
        /// 割当容量
        /// </summary>
        private long _AllocCount;


        /// <summary>
        /// 1時間あたりの生産量
        /// </summary>
        private long _ProductPerHour;


        /// <summary>
        /// 保管庫容量情報
        /// </summary>
        StorageCapacityInfo _CapacityInfo;
        #endregion


        #region プロパティ
        /// <summary>
        /// カーゴ種別ID
        /// </summary>
        public string TransportTypeID { get; }


        /// <summary>
        /// カーゴ種別名
        /// </summary>
        public string TransportTypeName { get; }


        /// <summary>
        /// ウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// ウェア名
        /// </summary>
        public string WareName { get; }


        /// <summary>
        /// ウェア大きさ
        /// </summary>
        public long Volume;


        /// <summary>
        /// 割当数量
        /// </summary>
        public long AllocCount
        {
            get => _AllocCount;
            set
            {
                if (SetProperty(ref _AllocCount, value))
                {
                    OnPropertyChanged(nameof(AllocCapacity));
                }
            }
        }

        /// <summary>
        /// 割当容量
        /// </summary>
        public long AllocCapacity => AllocCount * Volume;


        /// <summary>
        /// 割当可能容量
        /// </summary>
        public long AllocableCount => _CapacityInfo.FreeCapacity / Volume;


        /// <summary>
        /// 1時間あたりの生産量
        /// </summary>
        public long ProductPerHour
        {
            get => _ProductPerHour;
            set
            {
                if (SetProperty(ref _ProductPerHour, value))
                {
                    OnPropertyChanged(nameof(AfterCount));
                }
            }
        }


        /// <summary>
        /// 指定時間
        /// </summary>
        public long Hour
        {
            get => _Hour;
            set
            {
                if (SetProperty(ref _Hour, value))
                {
                    OnPropertyChanged(nameof(AfterCount));
                }
            }
        }


        /// <summary>
        /// 指定時間後の個数
        /// </summary>
        public long AfterCount => ProductPerHour * Hour;
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ware">ウェア</param>
        /// <param name="capacityInfo">保管庫容量情報</param>
        /// <param name="productPerHour">1時間あたりのウェア生産量</param>
        /// <param name="hour">指定時間</param>
        public StorageAssignGridItem(Ware ware, StorageCapacityInfo capacityInfo, long productPerHour, long hour)
        {
            WareID = ware.WareID;
            WareName = ware.Name;

            TransportTypeID = ware.TransportType.TransportTypeID;
            TransportTypeName = ware.TransportType.Name;

            Volume = ware.Volume;
            _CapacityInfo = capacityInfo;
            _CapacityInfo.PropertyChanged += CapacityInfo_PropertyChanged;

            ProductPerHour = productPerHour;
            Hour = hour;
        }

        /// <summary>
        /// 保管庫容量プロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CapacityInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(StorageCapacityInfo.FreeCapacity):
                    OnPropertyChanged(nameof(AllocableCount));
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _CapacityInfo.PropertyChanged -= CapacityInfo_PropertyChanged;
        }
    }
}

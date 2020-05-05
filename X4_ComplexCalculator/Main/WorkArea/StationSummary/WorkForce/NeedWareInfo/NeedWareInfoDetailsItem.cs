using System;
using System.Collections.Generic;
using System.Text;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.StationSummary.WorkForce.NeedWareInfo
{
    /// <summary>
    /// 必要ウェア詳細情報1レコード分
    /// </summary>
    class NeedWareInfoDetailsItem : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 必要数量
        /// </summary>
        private long _NeedAmount;

        /// <summary>
        /// 生産数量
        /// </summary>
        private long _ProductionAmount;
        #endregion

        /// <summary>
        /// 労働方式
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// ウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// 必要ウェア名
        /// </summary>
        public string WareName { get; }


        /// <summary>
        /// 必要数量
        /// </summary>
        public long NeedAmount
        {
            get => _NeedAmount;
            set
            {
                if (SetProperty(ref _NeedAmount, value))
                {
                    OnPropertyChanged(nameof(Diff));
                }
            }
        }

        /// <summary>
        /// 生産数量
        /// </summary>
        public long ProductionAmount
        {
            get => _ProductionAmount;
            set
            {
                if (SetProperty(ref _ProductionAmount, value))
                {
                    OnPropertyChanged(nameof(Diff));
                }
            }
        }


        /// <summary>
        /// 差
        /// </summary>
        public long Diff => ProductionAmount - NeedAmount;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="method">労働方式</param>
        /// <param name="wareID">ウェアID</param>
        /// <param name="wareName">ウェア名</param>
        /// <param name="needAmount">必要数量</param>
        /// <param name="productionAmount">生産数量</param>
        public NeedWareInfoDetailsItem(string method, string wareID, string wareName, long needAmount = 0, long productionAmount = 0)
        {
            Method           = method;
            WareID           = wareID;
            WareName         = wareName;
            NeedAmount       = needAmount;
            ProductionAmount = productionAmount;
        }
    }
}

using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSettings
{
    /// <summary>
    /// 労働者数を管理するクラス
    /// </summary>
    public class WorkforceManager : BindableBaseEx
    {
        #region メンバ
        /// <summary>
        /// 現在の労働者数
        /// </summary>
        private long _Actual = 0;


        /// <summary>
        /// 最大労働者数
        /// </summary>
        private long _Need = 0;


        /// <summary>
        /// 収容人数
        /// </summary>
        private long _Capacity = 0;


        /// <summary>
        /// 常に最大にするか
        /// </summary>
        private bool _AlwaysMaximum;
        #endregion


        #region プロパティ
        /// <summary>
        /// 現在の労働者数
        /// </summary>
        public long Actual
        {
            get => _Actual;
            set
            {
                var oldProportion = Proportion;
                if (SetPropertyEx(ref _Actual, value))
                {
                    RaisePropertyChangedEx(oldProportion, Proportion, nameof(Proportion));
                }
            }
        }


        /// <summary>
        /// 必要労働者数
        /// </summary>
        public long Need
        {
            get => _Need;
            set
            {
                var oldProportion = Proportion;
                if (SetPropertyEx(ref _Need, value))
                {
                    RaisePropertyChangedEx(oldProportion, Proportion, nameof(Proportion));
                }
            }
        }


        /// <summary>
        /// 収容人数
        /// </summary>
        public long Capacity
        {
            get => _Capacity;
            set
            {
                var isActualChange = value < Actual || Actual < value && AlwaysMaximum;
                if (SetPropertyEx(ref _Capacity, value) && isActualChange)
                {
                    Actual = value;
                }
            }
        }


        /// <summary>
        /// 現在の労働者数と必要労働者数の割合
        /// </summary>
        public double Proportion
        {
            get
            {
                if (Need == 0)
                {
                    return 0.0;
                }

                return (double)Actual / Need;
            }
        }


        /// <summary>
        /// 常に最大にするか
        /// </summary>
        public bool AlwaysMaximum
        {
            get => _AlwaysMaximum;
            set
            {
                if (SetProperty(ref _AlwaysMaximum, value) && value)
                {
                    Actual = Capacity;
                }
            }
        }
        #endregion


        /// <summary>
        /// 内容をクリアする
        /// </summary>
        public void Clear()
        {
            Need = 0;
            Capacity = 0;
            Actual = 0;
        }
    }
}

using System;
using Prism.Mvvm;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings
{
    /// <summary>
    /// ステーション設定用クラス
    /// </summary>
    public class StationSettings : BindableBase, IStationSettings
    {
        #region メンバ
        /// <summary>
        /// 本部か
        /// </summary>
        private bool _IsHeadquarters;


        /// <summary>
        /// 日光[%]
        /// </summary>
        private int _Sunlight = 100;
        #endregion


        #region プロパティ
        /// <summary>
        /// 本部か
        /// </summary>
        public bool IsHeadquarters
        {
            get => _IsHeadquarters;
            set => SetProperty(ref _IsHeadquarters, value);
        }


        /// <summary>
        /// 本部の必要労働者数
        /// </summary>
        public int HQWorkers { get; } = 200;


        /// <summary>
        /// 労働者
        /// </summary>
        public WorkforceManager Workforce { get; } = new WorkforceManager();


        /// <summary>
        /// 日光[%]
        /// </summary>
        public int Sunlight
        {
            get => _Sunlight;
            set
            {
                const int digit = 1;

                var tmp = Math.Pow(10.0, digit);

                SetProperty(ref _Sunlight, (int)(Math.Round(value / tmp, 0, MidpointRounding.AwayFromZero) * tmp));
            }
        }
        #endregion
    }
}

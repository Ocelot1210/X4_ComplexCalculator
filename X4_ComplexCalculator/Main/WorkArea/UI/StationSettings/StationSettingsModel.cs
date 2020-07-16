using Prism.Mvvm;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSettings
{
    /// <summary>
    /// ステーション設定用Model
    /// </summary>
    class StationSettingsModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 本部か
        /// </summary>
        private bool _IsHeadquarters;

        /// <summary>
        /// 採掘船を割り当て
        /// </summary>
        private bool _IsAssignMiner;

        /// <summary>
        /// 不足リソースを他ステーションから供給
        /// </summary>
        private bool _IsSupplyingScareResourcesFromOtherStations;

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
        public const int HQ_WORKERS = 200;


        /// <summary>
        /// 採掘船を割り当て
        /// </summary>
        public bool IsAssignMiner
        {
            get => _IsAssignMiner;
            set => SetProperty(ref _IsAssignMiner, value);
        }


        /// <summary>
        /// 不足リソースを他ステーションから供給
        /// </summary>
        public bool IsSupplyingScareResourcesFromOtherStations
        {
            get => _IsSupplyingScareResourcesFromOtherStations;
            set => SetProperty(ref _IsSupplyingScareResourcesFromOtherStations, value);
        }


        /// <summary>
        /// 日光[%]
        /// </summary>
        public int Sunlight
        {
            get => _Sunlight;
            set => SetProperty(ref _Sunlight, value);
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public StationSettingsModel()
        {

        }
    }
}

using Prism.Mvvm;

namespace X4_ComplexCalculator.Main.WorkArea.ResourcesGrid
{
    /// <summary>
    /// 建造コスト詳細表示用ListView1レコード分
    /// </summary>
    public class ResourcesGridDetailsItem : BindableBase
    {
        #region メンバ
        /// <summary>
        /// モジュール/装備数
        /// </summary>
        private long _Count;
        #endregion

        #region プロパティ
        /// <summary>
        /// モジュール/装備ID
        /// </summary>
        public string ID { get; }


        /// <summary>
        /// モジュール/装備名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// モジュール/装備1つ分生産に必要なウェア量
        /// </summary>
        public long Amount { get; }


        /// <summary>
        /// モジュール/装備数
        /// </summary>
        public long Count
        {
            get => _Count;
            set
            {
                if (SetProperty(ref _Count, value))
                {
                    RaisePropertyChanged(nameof(TotalAmount));
                }
            }
        }

        /// <summary>
        /// モジュール/装備生産に必要な総ウェア数
        /// </summary>
        public long TotalAmount => Amount * Count;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">モジュール/装備ID</param>
        /// <param name="name">モジュール/装備名</param>
        /// <param name="amount">モジュール/装備1つ分生産に必要なウェア量</param>
        /// <param name="count">モジュール/装備数</param>
        public ResourcesGridDetailsItem(string id, string name, long amount, long count = 0)
        {
            ID     = id;
            Name   = name;
            Amount = amount;
            Count  = count;
        }
    }
}

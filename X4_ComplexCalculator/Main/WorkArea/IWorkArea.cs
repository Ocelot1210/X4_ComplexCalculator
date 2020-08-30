using X4_ComplexCalculator.Main.WorkArea.WorkAreaData;

namespace X4_ComplexCalculator.Main.WorkArea
{
    interface IWorkArea
    {
        /// <summary>
        /// タイトル文字列
        /// </summary>
        public string Title { get; set; }


        /// <summary>
        /// 計算機で使用するステーション用データ
        /// </summary>
        public IStationData StationData { get; }
    }
}

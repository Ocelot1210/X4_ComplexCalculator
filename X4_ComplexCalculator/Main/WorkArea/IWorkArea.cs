using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.StationSettings;
using X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;
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
        public StationData StationData { get; }
    }
}

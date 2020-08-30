using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData
{
    /// <summary>
    /// 建造リソース情報用インターフェイス
    /// </summary>
    interface IBuildResourcesInfo
    {
        /// <summary>
        /// 建造リソース情報
        /// </summary>
        public ObservablePropertyChangedCollection<BuildResourcesGridItem> BuildResources { get; }
    }
}

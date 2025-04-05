using X4_ComplexCalculator.Common.Collections;
using X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData.BuildResources;

/// <summary>
/// 建造リソース情報用クラス
/// </summary>
public sealed class BuildResourcesInfo
{
    /// <summary>
    /// 建造リソース情報
    /// </summary>
    public ObservableRangeCollection<BuildResourcesGridItem> BuildResources { get; } = new();
}

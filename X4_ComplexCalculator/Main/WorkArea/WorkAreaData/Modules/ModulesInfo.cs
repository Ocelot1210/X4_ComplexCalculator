using X4_ComplexCalculator.Common.Collections;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;

/// <summary>
/// モジュール一覧情報用クラス
/// </summary>
public sealed class ModulesInfo
{
    /// <summary>
    /// モジュール一覧
    /// </summary>
    public ObservableRangeCollection<ModulesGridItem> Modules { get; } = [];
}

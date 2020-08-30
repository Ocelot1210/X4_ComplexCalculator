using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules
{
    /// <summary>
    /// モジュール一覧用インターフェイス
    /// </summary>
    interface IModulesInfo
    {
        /// <summary>
        /// モジュール一覧情報
        /// </summary>
        public ObservablePropertyChangedCollection<ModulesGridItem> Modules { get; }
    }
}

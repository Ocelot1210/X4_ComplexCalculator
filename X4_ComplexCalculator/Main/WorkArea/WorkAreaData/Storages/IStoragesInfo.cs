using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.UI.StoragesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Storages
{
    /// <summary>
    /// 保管庫情報用インターフェイス
    /// </summary>
    public interface IStoragesInfo
    {
        /// <summary>
        /// 保管庫情報
        /// </summary>
        public ObservablePropertyChangedCollection<StoragesGridItem> Storages { get; }
    }
}

using Prism.Mvvm;
using System.Windows.Data;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB;
using System.ComponentModel;
using System.Linq;


namespace X4_ComplexCalculator.Main.Menu.View.DBViewer.Wares
{
    /// <summary>
    /// ウェア情報閲覧用ViewModel
    /// </summary>
    class WaresViewModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// ウェア一覧
        /// </summary>
        private readonly ObservableRangeCollection<WaresGridItem> _Wares = new (Ware.GetAll().Select(x => new WaresGridItem(x)));
        #endregion


        #region プロパティ
        /// <summary>
        /// 表示用データ
        /// </summary>
        public ListCollectionView WaresView { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WaresViewModel()
        {
            WaresView = (ListCollectionView)CollectionViewSource.GetDefaultView(_Wares);
            WaresView.SortDescriptions.Clear();
            WaresView.SortDescriptions.Add(new SortDescription(nameof(WaresGridItem.WareName), ListSortDirection.Ascending));
        }
    }
}

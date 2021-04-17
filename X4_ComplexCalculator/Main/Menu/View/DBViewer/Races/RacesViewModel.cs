using Prism.Mvvm;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.Menu.View.DBViewer.Races
{
    /// <summary>
    /// 種族閲覧用ViewModel
    /// </summary>
    class RacesViewModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 種族一覧
        /// </summary>
        private readonly ObservableRangeCollection<RacesGridItem> _Races = new(X4Database.Instance.Race.GetAll().Select(x => new RacesGridItem(x)));
        #endregion


        #region プロパティ
        /// <summary>
        /// 表示用データ
        /// </summary>
        public ListCollectionView RacesView { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RacesViewModel()
        {
            RacesView = (ListCollectionView)CollectionViewSource.GetDefaultView(_Races);
            RacesView.SortDescriptions.Clear();
            RacesView.SortDescriptions.Add(new SortDescription(nameof(IRace.Name), ListSortDirection.Ascending));
        }
    }
}

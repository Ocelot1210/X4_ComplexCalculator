using Prism.Mvvm;
using System.Windows.Data;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB;
using System.ComponentModel;
using System.Linq;


namespace X4_ComplexCalculator.Main.Menu.View.DBViewer.Ships
{
    /// <summary>
    /// 艦船情報用ViewModel
    /// </summary>
    class ShipsViewModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// ウェア一覧
        /// </summary>
        private readonly ObservableRangeCollection<ShipsGridItem> _Wares = new(Ship.GetAll().Select(x => new ShipsGridItem(x)));
        #endregion


        #region プロパティ
        /// <summary>
        /// 表示用データ
        /// </summary>
        //public ListCollectionView WaresView { get; }
        #endregion
    }
}

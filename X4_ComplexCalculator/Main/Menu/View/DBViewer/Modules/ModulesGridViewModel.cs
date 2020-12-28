using Prism.Mvvm;
using System.Windows.Data;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB;
using System.ComponentModel;
using System.Linq;

namespace X4_ComplexCalculator.Main.Menu.View.DBViewer.Modules
{
    /// <summary>
    /// モジュール情報閲覧用ViewModel
    /// </summary>
    class ModulesGridViewModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// モジュール一覧
        /// </summary>
        private readonly ObservableRangeCollection<ModulesGridItem> _Modules = new(Module.GetAll().Where(x => !x.NoBluePrint).Select(x => new ModulesGridItem(x)));
        #endregion


        #region プロパティ
        /// <summary>
        /// 表示用データ
        /// </summary>
        public ListCollectionView ModulesView { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModulesGridViewModel()
        {
            ModulesView = (ListCollectionView)CollectionViewSource.GetDefaultView(_Modules);
            ModulesView.SortDescriptions.Clear();
            ModulesView.SortDescriptions.Add(new SortDescription(nameof(ModulesGridItem.ModuleName), ListSortDirection.Ascending));
        }

    }
}

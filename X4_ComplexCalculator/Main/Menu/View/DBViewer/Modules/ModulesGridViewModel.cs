using Prism.Mvvm;
using System.Windows.Data;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB;
using System.ComponentModel;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.DB;

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
        private readonly ObservableRangeCollection<ModulesGridItem> _Modules;
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
            var items = X4Database.Instance.Ware.GetAll<IX4Module>()
                .Where(x => !x.Tags.Contains("noplayerblueprint"))
                .Select(x => new ModulesGridItem(x));

            _Modules = new(items);

            ModulesView = (ListCollectionView)CollectionViewSource.GetDefaultView(_Modules);
            ModulesView.SortDescriptions.Clear();
            ModulesView.SortDescriptions.Add(new SortDescription(nameof(ModulesGridItem.ModuleName), ListSortDirection.Ascending));
        }

    }
}

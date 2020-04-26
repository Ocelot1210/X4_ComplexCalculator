using System.Collections.ObjectModel;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.ModulesGrid;

namespace X4_ComplexCalculator.Main.StoragesGrid
{
    /// <summary>
    /// 保管庫一覧表示用DataGridViewのViewModel
    /// </summary>
    class StoragesGridViewModel
    {
        #region メンバ
        /// <summary>
        /// 保管庫一覧表示用DataGridViewのModel
        /// </summary>
        readonly StoragesGridModel Model;
        #endregion


        #region プロパティ
        /// <summary>
        /// ストレージ一覧
        /// </summary>
        public ObservableCollection<StoragesGridItem> Storages => Model.Storages;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">モジュール一覧</param>
        public StoragesGridViewModel(ObservablePropertyChangedCollection<ModulesGridItem> modules)
        {
            Model = new StoragesGridModel(modules);
        }
    }
}

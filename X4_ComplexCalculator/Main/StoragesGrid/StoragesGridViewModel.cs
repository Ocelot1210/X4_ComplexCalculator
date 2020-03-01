using System.Collections.ObjectModel;
using X4_ComplexCalculator.Main.ModulesGrid;

namespace X4_ComplexCalculator.Main.StorageGrid
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
        /// <param name="moduleGridModel">モジュール一覧用Model</param>
        public StoragesGridViewModel(ModulesGridModel moduleGridModel)
        {
            Model = new StoragesGridModel(moduleGridModel);
        }
    }
}

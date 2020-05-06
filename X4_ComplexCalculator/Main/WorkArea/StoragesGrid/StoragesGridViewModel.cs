using System.Collections.ObjectModel;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.ModulesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.StoragesGrid
{
    /// <summary>
    /// 保管庫一覧表示用DataGridViewのViewModel
    /// </summary>
    class StoragesGridViewModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 保管庫一覧表示用DataGridViewのModel
        /// </summary>
        readonly StoragesGridModel _Model;
        #endregion


        #region プロパティ
        /// <summary>
        /// ストレージ一覧
        /// </summary>
        public ObservableCollection<StoragesGridItem> Storages => _Model.Storages;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="model">保管庫一覧表示用Model</param>
        public StoragesGridViewModel(StoragesGridModel model)
        {
            _Model = model;
        }

        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _Model.Dispose();
        }
    }
}

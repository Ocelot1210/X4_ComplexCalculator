using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StoragesGrid
{
    /// <summary>
    /// 保管庫一覧表示用DataGridViewのViewModel
    /// </summary>
    class StoragesGridViewModel : BindableBase, IDisposable
    {
        #region メンバ
        /// <summary>
        /// 保管庫一覧表示用DataGridViewのModel
        /// </summary>
        private readonly StoragesGridModel _Model;
        #endregion


        #region プロパティ
        /// <summary>
        /// ストレージ一覧
        /// </summary>
        public ObservableCollection<StoragesGridItem> Storages => _Model.Storages;


        /// <summary>
        /// 選択されたアイテムの展開/折りたたみ状態を設定する
        /// </summary>
        public ICommand SetSelectedExpandedCommand { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="model">保管庫一覧表示用Model</param>
        public StoragesGridViewModel(StoragesGridModel model)
        {
            _Model = model;
            SetSelectedExpandedCommand = new DelegateCommand<bool?>(SetSelectedExpanded);
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _Model.Dispose();
        }


        /// <summary>
        /// 選択されたアイテムの展開/折りたたみ状態を設定する
        /// </summary>
        /// <param name="param"></param>
        private void SetSelectedExpanded(bool? param)
        {
            foreach (var item in Storages.Where(x => x.IsSelected))
            {
                item.IsExpanded = param == true;
            }
        }
    }
}

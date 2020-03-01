using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Windows.Data;
using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.ModulesGrid
{
    class ModulesGridViewModel : INotifyPropertyChangedBace
    {
        #region メンバ変数
        /// <summary>
        /// Model
        /// </summary>
        private readonly ModulesGridModel Model;

        /// <summary>
        /// 検索モジュール名
        /// </summary>
        private string _SearchModuleName = "";

        /// <summary>
        /// 検索用フィルタを削除できるか
        /// </summary>
        private bool CanRemoveFilter = false;
        #endregion


        #region プロパティ
        /// <summary>
        /// 表示するコレクション
        /// </summary>
        private CollectionViewSource ModulesViewSource { get; set; }


        /// <summary>
        /// モジュール一覧
        /// </summary>
        public ObservableCollection<ModulesGridItem> Modules => Model.Modules;


        /// <summary>
        /// 検索するモジュール名
        /// </summary>
        public string SearchModuleName
        {
            get { return _SearchModuleName; }
            set
            {
                if (_SearchModuleName == value) return;
                _SearchModuleName = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }


        /// <summary>
        /// モジュール追加ボタンクリック
        /// </summary>
        public DelegateCommand AddButtonClicked { get; }


        /// <summary>
        /// モジュール削除ボタンクリック
        /// </summary>
        public DelegateCommand<ModulesGridItem> DeleteModule { get; }


        /// <summary>
        /// モジュール変更
        /// </summary>
        public DelegateCommand<ModulesGridItem> ReplaceModule { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="model">モデル</param>
        /// <param name="modulesViewSource">モジュール一覧</param>
        public ModulesGridViewModel(ModulesGridModel model, CollectionViewSource modulesViewSource)
        {
            Model = model;
            ModulesViewSource = modulesViewSource;
            AddButtonClicked = new DelegateCommand(Model.ShowAddModuleWindow);
            DeleteModule = new DelegateCommand<ModulesGridItem>(Model.DeleteModule);
            ReplaceModule = new DelegateCommand<ModulesGridItem>(Model.ReplaceModule);
        }

        /// <summary>
        /// フィルタを適用
        /// </summary>
        private void ApplyFilter()
        {
            // 2回目以降か？
            if (CanRemoveFilter)
            {
                ModulesViewSource.Filter -= new FilterEventHandler(FilterEvent);
                ModulesViewSource.Filter += new FilterEventHandler(FilterEvent);
            }
            else
            {
                // 初回はこっち
                CanRemoveFilter = true;
                ModulesViewSource.Filter += new FilterEventHandler(FilterEvent);
            }
        }


        /// <summary>
        /// フィルタイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ev"></param>
        private void FilterEvent(object sender, FilterEventArgs e)
        {
            e.Accepted = (e.Item is ModulesGridItem src && (SearchModuleName == "" || 0 <= src.Module.Name.IndexOf(SearchModuleName, StringComparison.InvariantCultureIgnoreCase)));
        }
    }
}

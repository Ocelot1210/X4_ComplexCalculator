using Prism.Commands;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using X4_ComplexCalculator.Common;
using System;
using X4_ComplexCalculator.Common.Collection;

namespace X4_ComplexCalculator.Main.ModulesGrid.SelectModule
{
    class SelectModuleViewModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// モデル
        /// </summary>
        readonly SelectModuleModel Model;


        /// <summary>
        /// 検索モジュール名
        /// </summary>
        private string _SearchModuleName = "";


        /// <summary>
        /// 検索用フィルタを削除できるか
        /// </summary>
        private bool CanRemoveFilter = false;


        /// <summary>
        /// 置換モードか
        /// </summary>
        private readonly bool IsReplaceMode;


        /// <summary>
        /// モジュール一覧を表示するコレクション
        /// </summary>
        private CollectionViewSource ModulesViewSource;
        #endregion


        #region プロパティ
        /// <summary>
        /// 変更前のモジュール名
        /// </summary>
        public string PrevModuleName { get; }

        /// <summary>
        /// 変更前モジュールを表示するか
        /// </summary>
        public Visibility PrevModuleVisiblity => (IsReplaceMode) ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// モジュール種別
        /// </summary>
        public SmartCollection<ModulesListItem> ModuleTypes { get { return Model.ModuleTypes; } }


        /// <summary>
        /// モジュール所有派閥
        /// </summary>
        public SmartCollection<ModulesListItem> ModuleOwners { get { return Model.ModuleOwners; } }


        /// <summary>
        /// モジュール一覧
        /// </summary>
        public SmartCollection<ModulesListItem> Modules { get { return Model.Modules; } }

        /// <summary>
        /// モジュール名検索用
        /// </summary>
        public string SearchModuleName
        {
            private get { return _SearchModuleName; }
            set
            {
                if (_SearchModuleName == value) return;
                _SearchModuleName = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }


        /// <summary>
        /// 選択ボタンクリック時
        /// </summary>
        public DelegateCommand<Window> OKButtonClickedCommand { get; }


        /// <summary>
        /// 閉じるボタンクリック時
        /// </summary>
        public DelegateCommand<Window> CloseButtonClickedCommand { get; }


        /// <summary>
        /// モジュール一覧ListBoxの選択モード
        /// </summary>
        public SelectionMode ModuleListSelectionMode => IsReplaceMode ? SelectionMode.Single : SelectionMode.Extended;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modules">選択結果格納先</param>
        /// <param name="isReplaceMode">置換モードか(falseで複数選択許可)</param>
        /// <param name="viewSource">modulesViewSource</param>
        public SelectModuleViewModel(SmartCollection<ModulesGridItem> modules, CollectionViewSource modulesViewSource, string prevModuleName = "")
        {
            Model = new SelectModuleModel(modules);

            PrevModuleName = prevModuleName;
            IsReplaceMode = prevModuleName != "";

            ModulesViewSource = modulesViewSource;
            OKButtonClickedCommand = new DelegateCommand<Window>(OKButtonClicked);
            CloseButtonClickedCommand = new DelegateCommand<Window>(CloseWindow);
        }


        /// <summary>
        /// ウィンドウが閉じられる時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnWindowClosing(object sender, EventArgs e)
        {
            Model.SaveCheckState();
        }


        /// <summary>
        /// OKボタンクリック時
        /// </summary>
        /// <param name="window">親ウィンドウ</param>
        private void OKButtonClicked(Window window)
        {
            Model.AddSelectedModuleToItemCollection();

            // 置換モードならウィンドウを閉じる
            if (IsReplaceMode)
            {
                window.Close();
            }
        }


        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        /// <param name="window">親ウィンドウ</param>
        private void CloseWindow(Window window)
        {
            window.Close();
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
            if (e.Item is ModulesListItem src)
            {
                // 選択解除する
                src.Checked = false;
                e.Accepted = src != null && (SearchModuleName == "" || 0 <= src.Name.IndexOf(SearchModuleName, StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}

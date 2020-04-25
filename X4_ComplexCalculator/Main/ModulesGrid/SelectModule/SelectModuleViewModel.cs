using Prism.Commands;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using X4_ComplexCalculator.Common;
using System;
using X4_ComplexCalculator.Common.Collection;
using System.ComponentModel;
using System.Windows.Input;

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
        /// 置換モードか
        /// </summary>
        private readonly bool IsReplaceMode;


        /// <summary>
        /// ウィンドウの表示状態
        /// </summary>
        private bool _CloseWindow = false;
        #endregion


        #region プロパティ
        /// <summary>
        /// ウィンドウの表示状態
        /// </summary>
        public bool CloseWindowProperty
        {
            get
            {
                return _CloseWindow;
            }
            set
            {
                _CloseWindow = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// ウィンドウが閉じられる時のコマンド
        /// </summary>
        public ICommand WindowClosingCommand { get; }


        /// <summary>
        /// 変更前のモジュール名
        /// </summary>
        public string PrevModuleName { get; }


        /// <summary>
        /// モジュール一覧表示用
        /// </summary>
        public ListCollectionView ModulesView { get; }


        /// <summary>
        /// 変更前モジュールを表示するか
        /// </summary>
        public Visibility PrevModuleVisiblity => IsReplaceMode ? Visibility.Visible : Visibility.Collapsed;


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
                if (_SearchModuleName != value)
                {
                    _SearchModuleName = value;
                    OnPropertyChanged();
                    ModulesView.Refresh();
                }
            }
        }


        /// <summary>
        /// 選択ボタンクリック時
        /// </summary>
        public ICommand OKButtonClickedCommand { get; }


        /// <summary>
        /// 閉じるボタンクリック時
        /// </summary>
        public ICommand CloseButtonClickedCommand { get; }


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
        public SelectModuleViewModel(SmartCollection<ModulesGridItem> modules, string prevModuleName = "")
        {
            Model = new SelectModuleModel(modules);

            PrevModuleName = prevModuleName;
            IsReplaceMode  = prevModuleName != "";

            ModulesView = (ListCollectionView)CollectionViewSource.GetDefaultView(Modules);
            ModulesView.Filter = Filter;

            OKButtonClickedCommand    = new DelegateCommand(OKButtonClicked);
            CloseButtonClickedCommand = new DelegateCommand(CloseWindow);
            WindowClosingCommand      = new DelegateCommand<CancelEventArgs>(WindowClosing);
        }


        /// <summary>
        /// ウィンドウが閉じられる時のイベント
        /// </summary>
        /// <param name="e"></param>
        public void WindowClosing(CancelEventArgs _)
        {
            Model.SaveCheckState();
        }


        /// <summary>
        /// OKボタンクリック時
        /// </summary>
        private void OKButtonClicked()
        {
            Model.AddSelectedModuleToItemCollection();

            // 置換モードならウィンドウを閉じる
            if (IsReplaceMode)
            {
                CloseWindowProperty = true;
            }
        }


        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        private void CloseWindow()
        {
            CloseWindowProperty = true;
        }


        /// <summary>
        /// フィルタイベント
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool Filter(object obj)
        {
            return obj is ModulesListItem src && (SearchModuleName == "" || 0 <= src.Name.IndexOf(SearchModuleName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}

using Prism.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using X4_ComplexCalculator.Common;
using System.Linq;

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

        /// <summary>
        /// コピーされたモジュール
        /// </summary>
        private IEnumerable<ModulesGridItem> CopiedModules;
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
            get
            {
                return _SearchModuleName;
            }
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
        /// モジュール変更
        /// </summary>
        public DelegateCommand<ModulesGridItem> ReplaceModule { get; }


        /// <summary>
        /// ソート順をクリア
        /// </summary>
        public DelegateCommand<DataGrid> ClearSortOrder { get; }


        /// <summary>
        /// モジュールをコピー
        /// </summary>
        public DelegateCommand CopyModules { get; }


        /// <summary>
        /// モジュールを貼り付け
        /// </summary>
        public DelegateCommand<DataGrid> PasteModules { get; }


        /// <summary>
        /// モジュールを削除
        /// </summary>
        public DelegateCommand<DataGrid> DeleteModules { get; }
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
            AddButtonClicked  = new DelegateCommand(Model.ShowAddModuleWindow);
            ReplaceModule     = new DelegateCommand<ModulesGridItem>(Model.ReplaceModule);
            ClearSortOrder    = new DelegateCommand<DataGrid>(ClearSortOrderCommand);
            CopyModules       = new DelegateCommand(CopyModulesCommand);
            PasteModules      = new DelegateCommand<DataGrid>(PasteModulesCommand);
            DeleteModules     = new DelegateCommand<DataGrid>(DeleteModulesCommand);
        }

        /// <summary>
        /// ソート順をクリア
        /// </summary>
        /// <param name="dataGrid"></param>
        private void ClearSortOrderCommand(DataGrid dataGrid)
        {
            var cvs = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            if (cvs != null)
            {
                cvs.SortDescriptions.Clear();
                foreach(var clm in dataGrid.Columns)
                {
                    clm.SortDirection = null;
                }
            }
        }

        /// <summary>
        /// 選択中のモジュールをコピー
        /// </summary>
        /// <param name="dataGrid"></param>
        private void CopyModulesCommand()
        {
            // 選択中のモジュールをコピー
            CopiedModules = CollectionViewSource.GetDefaultView(ModulesViewSource.View)
                                                .Cast<ModulesGridItem>()
                                                .Where(x => x.IsSelected)
                                                .Select(x => new ModulesGridItem(x));
        }

        /// <summary>
        /// コピーしたモジュールを貼り付け
        /// </summary>
        /// <param name="dataGrid"></param>
        private void PasteModulesCommand(DataGrid dataGrid)
        {
            Model.Modules.AddRange(CopiedModules.Select(x => new ModulesGridItem(x)).ToArray());
            dataGrid.Focus();
        }

        /// <summary>
        /// 選択中のモジュールを削除
        /// </summary>
        /// <param name="dataGrid"></param>
        private void DeleteModulesCommand(DataGrid dataGrid)
        {
            var cvs = (ListCollectionView)ModulesViewSource.View;
            var currPos = cvs.CurrentPosition;

            var items = CollectionViewSource.GetDefaultView(ModulesViewSource.View)
                                            .Cast<ModulesGridItem>()
                                            .Where(x => x.IsSelected)
                                            .ToArray();
            Model.DeleteModules(items);

            // 削除後に全部の選択状態を外さないと余計なものまで選択される
            foreach (var module in Model.Modules)
            {
                module.IsSelected = false;
            }

            // 最後の行を消した場合、選択行を最後にする
            if (cvs.Count <= currPos)
            {
                cvs.MoveCurrentToLast();
            }

            // 本当に選択したいものだけ選択
            if (cvs.CurrentItem is ModulesGridItem item)
            {
                item.IsSelected = true;
            }

            dataGrid.Focus();
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

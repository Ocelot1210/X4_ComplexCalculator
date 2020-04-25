using Prism.Commands;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.ModulesGrid
{
    class ModulesGridViewModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// Model
        /// </summary>
        private readonly ModulesGridModel _Model;


        /// <summary>
        /// 検索モジュール名
        /// </summary>
        private string _SearchModuleName = "";
        #endregion


        #region プロパティ
        /// <summary>
        /// モジュール一覧表示用
        /// </summary>
        public ListCollectionView ModulesView { get; }


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
                if (_SearchModuleName != value)
                {
                    _SearchModuleName = value;
                    OnPropertyChanged();
                    ModulesView.Refresh();
                }
            }
        }


        /// <summary>
        /// モジュール追加ボタンクリック
        /// </summary>
        public ICommand AddButtonClicked { get; }


        /// <summary>
        /// モジュール変更
        /// </summary>
        public ICommand ReplaceModule { get; }


        /// <summary>
        /// モジュールをコピー
        /// </summary>
        public ICommand CopyModules { get; }


        /// <summary>
        /// モジュールを貼り付け
        /// </summary>
        public ICommand PasteModules { get; }


        /// <summary>
        /// モジュールを削除
        /// </summary>
        public ICommand DeleteModules { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="model">モデル</param>
        public ModulesGridViewModel(ModulesGridModel model)
        {
            _Model = model;
            ModulesView        = (ListCollectionView)CollectionViewSource.GetDefaultView(_Model.Modules);
            ModulesView.Filter = Filter;
            AddButtonClicked   = new DelegateCommand(_Model.ShowAddModuleWindow);
            ReplaceModule      = new DelegateCommand<ModulesGridItem>(_Model.ReplaceModule);
            CopyModules        = new DelegateCommand(CopyModulesCommand);
            PasteModules       = new DelegateCommand<DataGrid>(PasteModulesCommand);
            DeleteModules      = new DelegateCommand<DataGrid>(DeleteModulesCommand);
        }


        /// <summary>
        /// 選択中のモジュールをコピー
        /// </summary>
        /// <param name="dataGrid"></param>
        private void CopyModulesCommand()
        {
            var xml = new XElement("modules");
            var selectedModules = CollectionViewSource.GetDefaultView(ModulesView)
                                                      .Cast<ModulesGridItem>()
                                                      .Where(x => x.IsSelected);
            
            foreach (var module in selectedModules)
            {
                xml.Add(module.ToXml());
            }

            Clipboard.SetText(xml.ToString());
        }


        /// <summary>
        /// コピーしたモジュールを貼り付け
        /// </summary>
        /// <param name="dataGrid"></param>
        private void PasteModulesCommand(DataGrid dataGrid)
        {
            try
            {
                var xml = XDocument.Parse(Clipboard.GetText());

                // xmlの内容に問題がないか確認するため、ここでToArray()する
                var modules = xml.Root.Elements().Select(x => new ModulesGridItem(x)).ToArray();

                _Model.Modules.AddRange(modules);
                dataGrid.Focus();
            }
            catch
            {
                
            }
        }

        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private DataGridCell GetCell(DataGrid grid, DataGridRow row, int column)
        {
            if (row != null)
            {
                var presenter = FindVisualChild<DataGridCellsPresenter>(row);
                if (presenter == null)
                {
                    row.ApplyTemplate();
                    presenter = FindVisualChild<DataGridCellsPresenter>(row);
                }
                if (presenter != null)
                {
                    var cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                    if (cell != null)
                    {
                        grid.ScrollIntoView(row, grid.Columns[column]);
                        cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                    }
                    return cell;
                }
            }

            return null;
        }

        /// <summary>
        /// 選択中のモジュールを削除
        /// </summary>
        /// <param name="dataGrid"></param>
        private void DeleteModulesCommand(DataGrid dataGrid)
        {
            var currPos = ModulesView.CurrentPosition;

            var items = CollectionViewSource.GetDefaultView(ModulesView)
                                            .Cast<ModulesGridItem>()
                                            .Where(x => x.IsSelected);
            _Model.DeleteModules(items);

            // 削除後に全部の選択状態を外さないと余計なものまで選択される
            foreach (var module in _Model.Modules)
            {
                module.IsSelected = false;
            }

            // 選択行を設定
            if (currPos < 0)
            {
                // 先頭行を削除した場合
                ModulesView.MoveCurrentToFirst();
            }
            else if (ModulesView.Count <= currPos)
            {
                // 最後の行を消した場合、選択行を最後にする
                ModulesView.MoveCurrentToLast();
            }
            else
            {
                // 中間行の場合
                ModulesView.MoveCurrentToPosition(currPos);
            }

            // 再度選択
            if (ModulesView.CurrentItem is ModulesGridItem item)
            {
                item.IsSelected = true;
            }

            // 選択項目があるか？
            if (0 <= ModulesView.CurrentPosition)
            {
                // UpdateLayout()とScrollIntoView()しないとrowが取れない！
                dataGrid.UpdateLayout();
                dataGrid.ScrollIntoView(dataGrid.Items[ModulesView.CurrentPosition]);

                // 参考：https://blog.magnusmontin.net/2013/11/08/how-to-programmatically-select-and-focus-a-row-or-cell-in-a-datagrid-in-wpf/
                var row = dataGrid.ItemContainerGenerator.ContainerFromIndex(ModulesView.CurrentPosition) as DataGridRow;
                if (row != null)
                {
                    var cell = GetCell(dataGrid, row, 0);
                    if (cell != null)
                    {
                        cell.Focus();
                    }
                }
            }
        }


        /// <summary>
        /// フィルタイベント
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool Filter(object obj)
        {
            return obj is ModulesGridItem src && (SearchModuleName == "" || 0 <= src.Module.Name.IndexOf(SearchModuleName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}

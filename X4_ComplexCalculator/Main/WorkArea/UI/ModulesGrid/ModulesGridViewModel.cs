using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid
{
    class ModulesGridViewModel : BindableBase, IDisposable
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
                if (SetProperty(ref _SearchModuleName, value))
                {
                    ModulesView.Refresh();
                }
            }
        }


        /// <summary>
        /// モジュール追加ボタンクリック
        /// </summary>
        public ICommand AddModuleCommand { get; }


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


        /// <summary>
        /// セルフォーカス用のコマンド
        /// </summary>
        public ICommand? CellFocusCommand { private get; set; }


        /// <summary>
        /// モジュールマージコマンド
        /// </summary>
        public ICommand MergeModuleCommand { get; }


        /// <summary>
        /// モジュール自動追加コマンド
        /// </summary>
        public ICommand? AutoAddModuleCommand { get; set; }
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
            AddModuleCommand   = new DelegateCommand(_Model.ShowAddModuleWindow);
            MergeModuleCommand = new DelegateCommand(_Model.MergeModule);
            ReplaceModule      = new DelegateCommand<ModulesGridItem>(_Model.ReplaceModule);
            CopyModules        = new DelegateCommand(CopyModulesCommand);
            PasteModules       = new DelegateCommand<DataGrid>(PasteModulesCommand);
            DeleteModules      = new DelegateCommand<DataGrid>(DeleteModulesCommand);
        }

        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _Model.Dispose();
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


        /// <summary>
        /// 選択中のモジュールを削除
        /// </summary>
        /// <param name="dataGrid"></param>
        private void DeleteModulesCommand(DataGrid dataGrid)
        {
            var currPos = ModulesView.CurrentPosition;

            // モジュール数を編集した後に削除するとcurrPosが-1になる場合があるため、
            // ここで最初に選択されている表示上のモジュールの要素番号を取得する
            if (currPos == -1)
            {
                var cnt = 0;
                foreach (var module in ModulesView.OfType<ModulesGridItem>())
                {
                    if (module.IsSelected)
                    {
                        currPos = cnt;
                        break;
                    }
                    cnt++;
                }
            }

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

            // セルフォーカス
            if (dataGrid.CurrentCell.Column != null)
            {
                CellFocusCommand?.Execute(new Tuple<DataGrid, int, int>(dataGrid, ModulesView.CurrentPosition, dataGrid.CurrentCell.Column.DisplayIndex));
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

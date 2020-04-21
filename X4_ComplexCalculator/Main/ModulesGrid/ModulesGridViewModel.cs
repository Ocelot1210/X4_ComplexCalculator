using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        public DelegateCommand AddButtonClicked { get; }


        /// <summary>
        /// モジュール変更
        /// </summary>
        public DelegateCommand<ModulesGridItem> ReplaceModule { get; }


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
            Parallel.ForEach(_Model.Modules, module => 
            {
                module.IsSelected = false;
            });

            // 先頭行を削除した場合
            if (currPos < 0)
            {
                ModulesView.MoveCurrentToFirst();
            }


            // 最後の行を消した場合、選択行を最後にする
            if (ModulesView.Count <= currPos)
            {
                ModulesView.MoveCurrentToLast();
            }


            // 本当に選択したいものだけ選択
            if (ModulesView.CurrentItem is ModulesGridItem item)
            {
                item.IsSelected = true;
            }

            dataGrid.Focus();
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

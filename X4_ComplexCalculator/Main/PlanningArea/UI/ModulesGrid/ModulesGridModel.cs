using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.PlanningArea.UI.ModulesGrid.SelectModule;

namespace X4_ComplexCalculator.Main.PlanningArea.UI.ModulesGrid
{
    class ModulesGridModel : IDisposable
    {
        #region メンバ
        /// <summary>
        /// モジュール選択ウィンドウ
        /// </summary>
        private SelectModuleWindow _SelectModuleWindow;

        /// <summary>
        /// モジュール選択ウィンドウがクローズ済みか
        /// </summary>
        private bool _SelectModuleWindowClosed = true;
        #endregion

        #region プロパティ
        /// <summary>
        /// モジュール一覧
        /// </summary>
        public ObservablePropertyChangedCollection<ModulesGridItem> Modules { get; private set; } = new ObservablePropertyChangedCollection<ModulesGridItem>();
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModulesGridModel()
        {
            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.PropertyChanged += LocalizeInstance_PropertyChanged;
        }

        /// <summary>
        /// 言語変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocalizeInstance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture))
            {
                // 言語変更時、ツールチップ文字列更新
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var module in Modules)
                    {
                        module.UpdateTooltip();
                    }
                });
                //System.Threading.Tasks.Parallel.ForEach(Modules, (module) =>
                //{
                //    module.UpdateTooltip();
                //});
            }
        }

        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            Modules.Clear();

            // モジュール選択ウィンドウが開いていたら閉じる
            if (!_SelectModuleWindowClosed)
            {
                _SelectModuleWindow?.Close();
            }

            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.PropertyChanged -= LocalizeInstance_PropertyChanged;
        }


        /// <summary>
        /// モジュール一括削除
        /// </summary>
        /// <param name="items">削除対象</param>
        public void DeleteModules(IEnumerable<ModulesGridItem> items)
        {
            Modules.RemoveRange(items);
        }

        /// <summary>
        /// モジュール追加画面を表示
        /// </summary>
        public void ShowAddModuleWindow()
        {
            if (_SelectModuleWindowClosed)
            {
                void OnWindowClosed(object s, EventArgs ev)
                {
                    _SelectModuleWindowClosed = true;
                }

                _SelectModuleWindow = new SelectModuleWindow(Modules);
                _SelectModuleWindow.Closed += OnWindowClosed;
                _SelectModuleWindow.Show();
            }
            _SelectModuleWindowClosed = false;

            _SelectModuleWindow.Activate();

            // 最小化されていたら通常状態にする
            _SelectModuleWindow.WindowState = (_SelectModuleWindow.WindowState == WindowState.Minimized)? WindowState.Normal : _SelectModuleWindow.WindowState;
        }


        /// <summary>
        /// モジュール変更
        /// </summary>
        /// <param name="oldItem">変更対象モジュール</param>
        public void ReplaceModule(ModulesGridItem oldItem)
        {
            // 置換後のモジュール
            var newModules = new ObservableRangeCollection<ModulesGridItem>();

            // 変更の場合はモーダル表示にする
            var wnd = new SelectModuleWindow(newModules, oldItem.Module.Name);
            wnd.ShowDialog();

            // 追加された場合
            if (0 < newModules.Count)
            {
                // 個数をコピーする
                var newItem = newModules.First();
                newItem.ModuleCount = oldItem.ModuleCount;

                // 要素を入れ替える
                Modules.Replace(oldItem, newItem);
            }
        }


        /// <summary>
        /// 同一モジュールをマージ
        /// </summary>
        public void MergeModule()
        {
            // モジュール数が1以下なら何もしない
            if (Modules.Count <= 1)
            {
                return;
            }

            var result = Localize.ShowMessageBox("Lang:MergeModulesConfirmMessage", "Lang:Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var dict = new Dictionary<int, (int, Module, ModuleProduction, long)>();

            var prevCnt = Modules.Count;

            foreach (var (module, idx) in Modules.Select((x, idx) => (x, idx)))
            {
                var hash = HashCode.Combine(module.Module, module.SelectedMethod);
                if (dict.ContainsKey(hash))
                {
                    var tmp = dict[hash];
                    tmp.Item4 += module.ModuleCount;
                    dict[hash] = tmp;
                }
                else
                {
                    dict.Add(hash, (idx, module.Module, module.SelectedMethod, module.ModuleCount));
                }
            }

            // モジュール数に変更があった場合のみ処理
            if (prevCnt != dict.Count)
            {
                Modules.Reset(dict.OrderBy(x => x.Value.Item1).Select(x => new ModulesGridItem(x.Value.Item2, x.Value.Item3, x.Value.Item4)));
                Localize.ShowMessageBox("Lang:MergeModulesMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, prevCnt - dict.Count);
            }
            else
            {
                Localize.ShowMessageBox("Lang:NoMergeModulesMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}

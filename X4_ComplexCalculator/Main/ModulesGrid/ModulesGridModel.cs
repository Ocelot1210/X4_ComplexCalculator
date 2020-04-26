using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.ModulesGrid.SelectModule;

namespace X4_ComplexCalculator.Main.ModulesGrid
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
        }


        /// <summary>
        /// モジュール一括削除
        /// </summary>
        /// <param name="items">削除対象</param>
        public void DeleteModules(IEnumerable<ModulesGridItem> items)
        {
            Modules.RemoveItems(items);
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
                    ((Window)s).Closed -= OnWindowClosed;
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
    }
}

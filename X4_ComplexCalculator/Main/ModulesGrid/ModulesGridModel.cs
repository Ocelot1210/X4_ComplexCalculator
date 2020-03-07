using System;
using System.Collections.Specialized;
using System.Windows;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Main.ModulesGrid.SelectModule;
using System.Linq;

namespace X4_ComplexCalculator.Main.ModulesGrid
{
    class ModulesGridModel
    {
        #region メンバ
        /// <summary>
        /// 親ウィンドウ
        /// </summary>
        private readonly Window OwnerWindow;

        /// <summary>
        /// モジュール選択ウィンドウ
        /// </summary>
        private SelectModuleWindow selectModuleWindow;
        #endregion

        #region プロパティ
        /// <summary>
        /// モジュール一覧
        /// </summary>
        public MemberChangeDetectCollection<ModulesGridItem> Modules { get; private set; } = new MemberChangeDetectCollection<ModulesGridItem>();

        /// <summary>
        /// プロパティ変更時のイベント
        /// </summary>
        public event NotifyCollectionChangedEventHandler OnModulesChanged
        {
            add
            {
                Modules.OnCollectionChangedMain += value;
            }
            remove
            {
                Modules.OnCollectionChangedMain -= value;
            }
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ownerWindow">親ウィンドウ</param>
        public ModulesGridModel(Window ownerWindow)
        {
            OwnerWindow = ownerWindow;
        }


        /// <summary>
        /// モジュールを削除
        /// </summary>
        /// <param name="item">削除したいモジュール</param>
        public void DeleteModule(ModulesGridItem item)
        {
            Modules.Remove(item);
        }

        /// <summary>
        /// モジュール一括削除
        /// </summary>
        public void DeleteModules()
        {
            Modules.RemoveItems(Modules.Where(x => x.IsSelected));
        }

        /// <summary>
        /// モジュール追加画面を表示
        /// </summary>
        public void ShowAddModuleWindow()
        {
            if (selectModuleWindow == null)
            {
                selectModuleWindow = new SelectModuleWindow(Modules, false);
                selectModuleWindow.Closed += (object s, EventArgs ev) => { selectModuleWindow.Close(); GC.Collect(); selectModuleWindow = null; };
                selectModuleWindow.Owner = OwnerWindow;
                selectModuleWindow.Show();
            }

            selectModuleWindow.Activate();

            // 最小化されていたら通常状態にする
            selectModuleWindow.WindowState = (selectModuleWindow.WindowState == WindowState.Minimized)? WindowState.Normal : selectModuleWindow.WindowState;
        }


        /// <summary>
        /// モジュール変更
        /// </summary>
        /// <param name="oldItem">変更対象モジュール</param>
        public void ReplaceModule(ModulesGridItem oldItem)
        {
            // 置換後のモジュール
            var newModules = new SmartCollection<ModulesGridItem>();

            // 変更の場合はモーダル表示にする
            var wnd = new SelectModuleWindow(newModules, true);
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

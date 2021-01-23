using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.SelectModule;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid
{
    class ModulesGridModel : IDisposable
    {
        #region メンバ
        /// <summary>
        /// モジュール選択ウィンドウ
        /// </summary>
        private SelectModuleWindow? _SelectModuleWindow;


        /// <summary>
        /// モジュール選択ウィンドウがクローズ済みか
        /// </summary>
        private bool _SelectModuleWindowClosed = true;


        /// <summary>
        /// モジュール一覧情報
        /// </summary>
        private readonly IModulesInfo _ModulesInfo;
        #endregion


        #region プロパティ
        /// <summary>
        /// モジュール一覧
        /// </summary>
        public ObservableRangeCollection<ModulesGridItem> Modules => _ModulesInfo.Modules;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModulesGridModel(IModulesInfo modulesInfo)
        {
            _ModulesInfo = modulesInfo;
            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.PropertyChanged += LocalizeInstance_PropertyChanged;
        }


        /// <summary>
        /// 言語変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocalizeInstance_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture))
            {
                // 言語変更時、ツールチップ文字列更新
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var module in _ModulesInfo.Modules)
                    {
                        module.UpdateEquipmentInfo();
                    }
                });
            }
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _ModulesInfo.Modules.Clear();

            // モジュール選択ウィンドウが開いていたら閉じる
            if (!_SelectModuleWindowClosed)
            {
                _SelectModuleWindow?.Close();
            }

            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.PropertyChanged -= LocalizeInstance_PropertyChanged;
        }


        /// <summary>
        /// モジュール追加画面を表示
        /// </summary>
        public void ShowAddModuleWindow()
        {
            if (_SelectModuleWindowClosed)
            {
                void OnWindowClosed(object? s, EventArgs ev)
                {
                    _SelectModuleWindowClosed = true;
                }

                _SelectModuleWindow = new SelectModuleWindow(_ModulesInfo.Modules);
                _SelectModuleWindow.Closed += OnWindowClosed;
                _SelectModuleWindow.Show();
            }
            _SelectModuleWindowClosed = false;


            if (_SelectModuleWindow is null)
            {
                throw new InvalidOperationException();
            }


            _SelectModuleWindow.Activate();

            // 最小化されていたら通常状態にする
            _SelectModuleWindow.WindowState = (_SelectModuleWindow.WindowState == WindowState.Minimized) ? WindowState.Normal : _SelectModuleWindow.WindowState;
        }


        /// <summary>
        /// モジュール変更
        /// </summary>
        /// <param name="oldItem">変更対象モジュール</param>
        public bool ReplaceModule(ModulesGridItem oldItem)
        {
            var ret = false;

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
                _ModulesInfo.Modules.Replace(oldItem, newItem);

                ret = true;
            }

            return ret;
        }


        /// <summary>
        /// 同一モジュールをマージ
        /// </summary>
        public void MergeModule()
        {
            // モジュール数が1以下なら何もしない
            if (_ModulesInfo.Modules.Count <= 1)
            {
                return;
            }

            var result = LocalizedMessageBox.Show("Lang:MergeModulesConfirmMessage", "Lang:Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var dict = new Dictionary<int, (int idx, ModulesGridItem Module)>();

            var prevCnt = Modules.Count;
            var mergedModules = 0L;

            foreach (var (module, idx) in Modules.Select((x, idx) => (x, idx)))
            {
                var hash = HashCode.Combine(module.Module, module.Equipments, module.SelectedMethod);
                if (dict.ContainsKey(hash))
                {
                    var tmp = dict[hash];
                    tmp.Module.ModuleCount += module.ModuleCount;
                    tmp.Module.EditStatus   = EditStatus.Edited;

                    mergedModules += module.ModuleCount;
                }
                else
                {
                    dict.Add(hash, (idx, new ModulesGridItem(module.ToXml()) { EditStatus = module.EditStatus }));
                }
            }

            // モジュール数に変更があった場合のみ処理
            if (prevCnt != dict.Count)
            {
                _ModulesInfo.Modules.Reset(dict.OrderBy(x => x.Value.idx).Select(x => x.Value.Module));
                LocalizedMessageBox.Show("Lang:MergeModulesMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, mergedModules, prevCnt - dict.Count);
            }
            else
            {
                LocalizedMessageBox.Show("Lang:NoMergeModulesMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}

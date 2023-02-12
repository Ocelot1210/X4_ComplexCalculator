using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Dialog.MessageBoxes;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.SelectModule;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

class ModulesGridModel : IDisposable
{
    #region メンバ
    /// <summary>
    /// モジュール一覧情報
    /// </summary>
    private readonly IModulesInfo _modulesInfo;


    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _localizedMessageBox;


    /// <summary>
    /// モジュール選択ウィンドウ
    /// </summary>
    private SelectModuleWindow? _selectModuleWindow;


    /// <summary>
    /// モジュール選択ウィンドウがクローズ済みか
    /// </summary>
    private bool _selectModuleWindowClosed = true;
    #endregion


    #region プロパティ
    /// <summary>
    /// モジュール一覧
    /// </summary>
    public ObservableRangeCollection<ModulesGridItem> Modules => _modulesInfo.Modules;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="modulesInfo">モジュール一覧</param>
    /// <param name="localizedMessageBox">メッセージボックス表示用</param>
    public ModulesGridModel(IModulesInfo modulesInfo, ILocalizedMessageBox localizedMessageBox)
    {
        _modulesInfo = modulesInfo;
        _localizedMessageBox = localizedMessageBox;
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
                foreach (var module in _modulesInfo.Modules)
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
        _modulesInfo.Modules.Clear();

        // モジュール選択ウィンドウが開いていたら閉じる
        if (!_selectModuleWindowClosed)
        {
            _selectModuleWindow?.Close();
        }

        WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.PropertyChanged -= LocalizeInstance_PropertyChanged;
    }


    /// <summary>
    /// モジュール追加画面を表示
    /// </summary>
    public void ShowAddModuleWindow()
    {
        if (_selectModuleWindowClosed)
        {
            void OnWindowClosed(object? s, EventArgs ev)
            {
                _selectModuleWindowClosed = true;
            }

            _selectModuleWindow = new SelectModuleWindow(_modulesInfo.Modules);
            _selectModuleWindow.Closed += OnWindowClosed;
            _selectModuleWindow.Show();
        }
        _selectModuleWindowClosed = false;


        if (_selectModuleWindow is null)
        {
            throw new InvalidOperationException();
        }


        _selectModuleWindow.Activate();

        // 最小化されていたら通常状態にする
        _selectModuleWindow.WindowState = (_selectModuleWindow.WindowState == WindowState.Minimized) ? WindowState.Normal : _selectModuleWindow.WindowState;
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
            _modulesInfo.Modules.Replace(oldItem, newItem);

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
        if (_modulesInfo.Modules.Count <= 1)
        {
            return;
        }

        var result = _localizedMessageBox.YesNo("Lang:Modules_Button_Merge_ConfirmMessage", "Lang:Common_MessageBoxTitle_Confirmation", LocalizedMessageBoxResult.No);
        if (result != LocalizedMessageBoxResult.Yes)
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
            _modulesInfo.Modules.Reset(dict.OrderBy(x => x.Value.idx).Select(x => x.Value.Module));
            _localizedMessageBox.Ok("Lang:Modules_Button_Merge_MergeModulesMessage", "Lang:Common_MessageBoxTitle_Confirmation", mergedModules, prevCnt - dict.Count);
        }
        else
        {
            _localizedMessageBox.Ok("Lang:Modules_Button_Merge_NoMergeModulesMessage", "Lang:Common_MessageBoxTitle_Confirmation");
        }
    }
}

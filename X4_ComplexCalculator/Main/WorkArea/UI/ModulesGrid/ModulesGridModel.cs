using Collections.Pooled;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Linq;
using System.Windows;
using X4_ComplexCalculator.Common.Collections;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.SelectModule;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

/// <summary>
/// コンストラクタ
/// </summary>
/// <param name="messanger">メッセージ交換用</param>
/// <param name="modulesInfo">モジュール一覧</param>
/// <param name="localizedMessageBox">メッセージボックス表示用</param>
class ModulesGridModel(IMessenger messenger, ModulesInfo modulesInfo, ILocalizedMessageBox localizedMessageBox) : ObservableRecipient(messenger), IDisposable
{
    #region メンバ
    /// <summary>
    /// モジュール一覧情報
    /// </summary>
    private readonly ModulesInfo _modulesInfo = modulesInfo;


    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _localizedMessageBox = localizedMessageBox;


    /// <summary>
    /// モジュール選択ウィンドウ
    /// </summary>
    private SelectModuleWindow? _selectModuleWindow;
    #endregion


    #region プロパティ
    /// <summary>
    /// モジュール一覧
    /// </summary>
    public ObservableRangeCollection<ModulesGridItem> Modules => _modulesInfo.Modules;
    #endregion


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _modulesInfo.Modules.Clear();

        // モジュール選択ウィンドウが開いていたら閉じる
        _selectModuleWindow?.Close();
    }


    /// <summary>
    /// モジュール追加画面を表示
    /// </summary>
    public void ShowAddModuleWindow()
    {
        if (_selectModuleWindow is null)
        {
            void OnWindowClosed(object? s, EventArgs ev)
            {
                _selectModuleWindow!.Closed -= OnWindowClosed;
                _selectModuleWindow = null;
            }

            _selectModuleWindow = new SelectModuleWindow(Messenger, _modulesInfo.Modules);
            _selectModuleWindow.Closed += OnWindowClosed;
        }

        _selectModuleWindow.Show();
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
        var wnd = new SelectModuleWindow(Messenger, newModules, oldItem.Module.Name);
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
    /// 不足するモジュールを自動追加
    /// </summary>
    public void AutoAddModule()
    {
        var result = _localizedMessageBox.YesNo("Lang:Modules_Button_AutoAdd_ConfirmMessage", "Lang:Common_MessageBoxTitle_Confirmation", LocalizedMessageBoxResult.No);
        if (result != LocalizedMessageBoxResult.Yes)
        {
            return;
        }

        var addedRecords = 0L;              // 追加レコード数
        var addedModules = 0L;              // 追加モジュール数

        // モジュール自動追加で追加されたモジュール一覧
        using var autoAddedModules = new PooledDictionary<string, ModulesGridItem>();


        while (true)
        {
            // 追加モジュールIDとモジュール数のペア一覧
            var addModules = Messenger.Send<RequestMessage<(IX4Module, long)[]>>().Response;

            // 追加モジュールが無ければ(不足が無くなれば)終了
            if (addModules.Length == 0)
            {
                break;
            }

            using var addTarget = new PooledList<ModulesGridItem>();      // 実際に追加するモジュール一覧

            foreach (var (module, count) in addModules)
            {
                // モジュール自動追加作業用に実際に追加するモジュールが存在するか？
                if (autoAddedModules.TryGetValue(module.ID, out ModulesGridItem? value))
                {
                    // モジュール自動追加作業用に実際に追加するモジュールが存在する場合、
                    // モジュール数を増やしてレコードがなるべく増えないようにする
                    value.ModuleCount += count;
                }
                else
                {
                    // モジュール自動追加作業用に実際に追加するモジュールが存在しない場合、
                    // 実際に追加するモジュールと見なす
                    var mgi = new ModulesGridItem(Messenger, module, null, count) { EditStatus = EditStatus.Edited };
                    addTarget.Add(mgi);
                    autoAddedModules.Add(module.ID, mgi);

                    // 追加レコード数更新
                    addedRecords++;
                }

                // 追加モジュール数更新
                addedModules += count;
            }

            // モジュール一覧に追加対象モジュールを追加
            Modules.AddRange(addTarget);
        }


        if (addedRecords == 0)
        {
            _localizedMessageBox.Ok("Lang:Modules_Button_AutoAdd_NoAddedModulesMessage", "Lang:Common_MessageBoxTitle_Confirmation");
        }
        else
        {
            _localizedMessageBox.Ok("Lang:Modules_Button_AutoAdd_AddedModulesMessage", "Lang:Common_MessageBoxTitle_Confirmation", addedRecords, addedModules);
        }
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

        using var dict = new PooledDictionary<int, (int idx, ModulesGridItem Module)>();

        var prevCnt = Modules.Count;
        var mergedModules = 0L;

        foreach (var (module, idx) in Modules.Select((x, idx) => (x, idx)))
        {
            var hash = HashCode.Combine(module.Module, module.Equipments, module.SelectedMethod);
            if (dict.TryGetValue(hash, out (int idx, ModulesGridItem Module) tmp))
            {
                tmp.Module.ModuleCount += module.ModuleCount;
                tmp.Module.EditStatus   = EditStatus.Edited;

                mergedModules += module.ModuleCount;
            }
            else
            {
                dict.Add(hash, (idx, new ModulesGridItem(Messenger, module.ToXml()) { EditStatus = module.EditStatus }));
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

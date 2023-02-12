using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using X4_ComplexCalculator.Common.Dialog.MessageBoxes;
using X4_ComplexCalculator.Common.Dialog.SelectStringDialog;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.Menu.Layout;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main;

/// <summary>
/// レイアウト管理用クラス
/// </summary>
class LayoutsManager : IDisposable
{
    #region メンバ
    /// <summary>
    /// 作業エリア管理用
    /// </summary>
    private readonly WorkAreaManager _workAreaManager;


    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _localizedMessageBox;


    /// <summary>
    /// 現在のレイアウト
    /// </summary>
    private readonly ReactivePropertySlim<LayoutMenuItem?> _activeLayout = new();


    /// <summary>
    /// Dispose が必要なオブジェクトのコレクション
    /// </summary>
    private readonly CompositeDisposable _disposables = new();
    #endregion


    #region プロパティ
    /// <summary>
    /// レイアウト一覧
    /// </summary>
    public ObservableCollection<LayoutMenuItem> Layouts { get; } = new();


    /// <summary>
    /// 現在のレイアウト
    /// </summary>
    public IReadOnlyReactiveProperty<LayoutMenuItem?> ActiveLayout => _activeLayout;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="workAreaManager">作業エリア管理用オブジェクト</param>
    /// <param name="localizedMessageBox">メッセージボックス表示用</param>
    public LayoutsManager(WorkAreaManager workAreaManager, ILocalizedMessageBox localizedMessageBox)
    {
        _workAreaManager = workAreaManager;
        _localizedMessageBox = localizedMessageBox;

        // レイアウト一覧の上書きボタンをトリガーにレイアウト上書き保存を実行
        Layouts.ObserveElementObservableProperty(x => x.SaveButtonClickedCommand)
            .Select(x => x.Instance)
            .Subscribe(OverwritedSaveLayout)
            .AddTo(_disposables);

        // レイアウト一覧の変更ボタンをトリガーにレイアウト名変更を実行
        Layouts.ObserveElementObservableProperty(x => x.EditButtonClickedCommand)
            .Select(x => x.Instance)
            .Subscribe(EditLayoutName)
            .AddTo(_disposables);

        // レイアウト一覧の削除ボタンをトリガーにレイアウト削除を実行
        Layouts.ObserveElementObservableProperty(x => x.DeleteButtonClickedCommand)
            .Select(x => x.Instance)
            .Subscribe(DeleteLayout)
            .AddTo(_disposables);

        // プリセットが選択、または解除された場合、DB に状態を保存する
        var changeChecked = Layouts.ObserveElementObservableProperty(x => x.IsChecked);
        changeChecked.Select(x => x.Instance)
            .Subscribe(x => SettingDatabase.Instance.Execute($"UPDATE WorkAreaLayouts SET IsChecked = :IsChecked WHERE LayoutID = :LayoutID", new { IsChecked = x.IsChecked.Value ? 1 : 0, x.LayoutID }));

        // プリセットが選択された場合、他のチェックを全部外す
        changeChecked.Where(x => x.Value)
            .Select(x => x.Instance)
            .Subscribe(ExclusiveChecked);
    }


    /// <summary>
    /// 初期化
    /// </summary>
    public void Init()
    {
        // レイアウト一覧読み込み
        const string SQL = "SELECT LayoutID, LayoutName, IsChecked FROM WorkAreaLayouts";
        var items = SettingDatabase.Instance.Query<(long LayoutID, string LayoutName, bool IsChecked)>(SQL);
        foreach (var (layoutID, layoutName, isChecked) in items)
        {
            Layouts.Add(new LayoutMenuItem(layoutID, layoutName, isChecked));
        }

        var checkedLayout = Layouts.FirstOrDefault(x => x.IsChecked.Value);
        if (checkedLayout is not null)
        {
            _activeLayout.Value = checkedLayout;
        }
    }


    /// <summary>
    /// レイアウト保存
    /// </summary>
    public void SaveLayout(WorkAreaViewModel? vm)
    {
        if (vm is not null)
        {
            var (onOK, layoutName) = SelectStringDialog.ShowDialog("Lang:MainWindow_Menu_Layout_MenuItem_SaveLayout_Title", "Lang:MainWindow_Menu_Layout_MenuItem_SaveLayout_Description", "", IsValidLayoutName);
            if (onOK)
            {
                try
                {
                    var layoutID = vm.LayoutManager.SaveLayout(layoutName);

                    Layouts.Add(new LayoutMenuItem(layoutID, layoutName, false));
                }
                catch (Exception ex)
                {
                    _localizedMessageBox.Error("Lang:MainWindow_Menu_Layout_MenuItem_SaveLayout_FailedMessage", "Lang:Common_MessageBoxTitle_Error", ex.Message);
                }

                _localizedMessageBox.Ok("Lang:MainWindow_Menu_Layout_MenuItem_SaveLayout_SucceededMessage", "Lang:Common_MessageBoxTitle_Confirmation", vm.Title, layoutName);
            }
        }
        else
        {
            _localizedMessageBox.Warn("Lang:MainWindow_Menu_Layout_MenuItem_SaveLayout_TabDoesNotSelectedMessage", "Lang:Common_MessageBoxTitle_Confirmation");
        }
    }


    /// <summary>
    /// レイアウト上書き保存
    /// </summary>
    /// <param name="menuItem">上書きするレイアウト</param>
    private void OverwritedSaveLayout(LayoutMenuItem menuItem)
    {
        if (_workAreaManager.ActiveContent is null) return;

        try
        {
            _workAreaManager.ActiveContent.LayoutManager.OverwriteSaveLayout(menuItem.LayoutID);

            _localizedMessageBox.Ok("Lang:MainWindow_Menu_Layout_MenuItem_LayoutList_Overwrite_SuccessMessage", "Lang:Common_MessageBoxTitle_Confirmation", _workAreaManager.ActiveContent.Title, menuItem.LayoutName);
        }
        catch (Exception ex)
        {
            _localizedMessageBox.Error("Lang:MainWindow_Menu_Layout_MenuItem_LayoutList_Overwrite_FailedMessage", "Lang:Common_MessageBoxTitle_Error", ex.Message);
        }
    }


    /// <summary>
    /// レイアウト名変更
    /// </summary>
    private void EditLayoutName(LayoutMenuItem menuItem)
    {
        var (onOK, newLayoutName) = SelectStringDialog.ShowDialog("Lang:MainWindow_Menu_Layout_MenuItem_LayoutList_Rename_Title", "Lang:MainWindow_Menu_Layout_MenuItem_LayoutList_Rename_Description", menuItem.LayoutName.Value, IsValidLayoutName);
        if (onOK && menuItem.LayoutName.Value != newLayoutName)
        {
            menuItem.LayoutName.Value = newLayoutName;

            const string SQL = "UPDATE WorkAreaLayouts SET LayoutName = :LayoutName WHERE LayoutID = :LayoutID";

            SettingDatabase.Instance.Execute(SQL, new { LayoutName = menuItem.LayoutName.Value, menuItem.LayoutID });
        }
    }


    /// <summary>
    /// レイアウト削除
    /// </summary>
    private void DeleteLayout(LayoutMenuItem menuItem)
    {
        var result = _localizedMessageBox.YesNo("Lang:MainWindow_Menu_Layout_MenuItem_LayoutList_DeleteLayoutButton_ConfirmMessage", "Lang:Common_MessageBoxTitle_Confirmation", LocalizedMessageBoxResult.No, menuItem.LayoutName);

        if (result == LocalizedMessageBoxResult.Yes)
        {
            SettingDatabase.Instance.Execute("DELETE FROM WorkAreaLayouts WHERE LayoutID = :LayoutID", new { menuItem.LayoutID });
            Layouts.Remove(menuItem);
            _activeLayout.Value = null;
        }
    }


    /// <summary>
    /// レイアウト名が有効か判定
    /// </summary>
    /// <param name="layoutName">レイアウト名</param>
    /// <returns>レイアウト名が有効か</returns>
    private bool IsValidLayoutName(string layoutName)
    {
        var ret = true;

        if (string.IsNullOrWhiteSpace(layoutName))
        {
            _localizedMessageBox.Warn("Lang:MainWindow_Menu_Layout_InvalidLayoutNameMessage", "Lang:Common_MessageBoxTitle_Confirmation");
            ret = false;
        }

        return ret;
    }


    /// <summary>
    /// プリセットが選択された場合、他のチェックを全部外す
    /// </summary>
    /// <param name="menuItem">選択されたプリセット</param>
    private void ExclusiveChecked(LayoutMenuItem menuItem)
    {
        if (menuItem.IsChecked.Value)
        {
            foreach (var layout in Layouts.Where(x => x != menuItem))
            {
                layout.IsChecked.Value = false;
            }

            _activeLayout.Value = menuItem;
        }
    }


    /// <inheritdoc />
    public void Dispose() => _disposables.Dispose();
}

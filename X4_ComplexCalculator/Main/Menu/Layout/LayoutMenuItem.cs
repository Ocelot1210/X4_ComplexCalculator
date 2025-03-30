using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;
using X4_ComplexCalculator.Common.Dialogs.SelectStringDialog;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main.Menu.Layout;

/// <summary>
/// レイアウト一覧の1レコード分
/// </summary>
public sealed partial class LayoutMenuItem : ObservableRecipientEx
{
    #region メンバ
    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _localizedMessageBox;
    #endregion


    #region プロパティ
    /// <summary>
    /// レイアウトID
    /// </summary>
    public long LayoutID { get; }


    /// <summary>
    /// レイアウト名
    /// </summary>
    [ObservableProperty]
    public partial string LayoutName { get; set; }


    /// <summary>
    /// チェック状態
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial bool IsChecked { get; set;  }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="layoutID">レイアウトID</param>
    /// <param name="layoutName">レイアウト名</param>
    /// <param name="isChecked">チェックされているか</param>
    public LayoutMenuItem(IMessenger messenger, ILocalizedMessageBox localizedMessageBox, long layoutID, string layoutName, bool isChecked) : base(messenger)
    {
        _localizedMessageBox = localizedMessageBox;

        LayoutID = layoutID;
        LayoutName = layoutName;
        IsChecked = isChecked;

        Messenger.RegisterPropertyChangedMessage(this, static (LayoutMenuItem x) => x.IsChecked, static (r, m) => r.UncheckIfNeeded(m));

        IsActive = true;
    }


    /// <summary>
    /// レイアウト名を作成する (ユーザに入力させる)
    /// </summary>
    /// <param name="orgName">レイアウト名初期値</param>
    /// <param name="localizedMessageBox">メッセージボックス表示用</param>
    /// <returns>有効なレイアウト名の場合、非 <c>null</c> 値。それ以外の場合 <c>null</c></returns>
    public static string? MakeLayoutName(string orgName, ILocalizedMessageBox localizedMessageBox)
    {
        var (onOK, layoutName) = SelectStringDialog.ShowDialog("Lang:MainWindow_Menu_Layout_MenuItem_LayoutList_Rename_Title", "Lang:MainWindow_Menu_Layout_MenuItem_LayoutList_Rename_Description", orgName, x => IsValidLayoutName(x, localizedMessageBox));

        return onOK ? layoutName : null;
    }


    /// <summary>
    /// レイアウト名が有効か判定
    /// </summary>
    /// <param name="layoutName">レイアウト名</param>
    /// <returns>レイアウト名が有効か</returns>
    private static bool IsValidLayoutName(string layoutName, ILocalizedMessageBox localizedMessageBox)
    {
        var ret = true;

        if (string.IsNullOrWhiteSpace(layoutName))
        {
            localizedMessageBox.Warn("Lang:MainWindow_Menu_Layout_InvalidLayoutNameMessage", "Lang:Common_MessageBoxTitle_Confirmation");
            ret = false;
        }

        return ret;
    }



    /// <summary>
    /// 保存ボタンクリック時にレイアウトを上書き保存
    /// </summary>
    [RelayCommand]
    private void OnSaveButtonClicked()
    {
        var vm = Messenger.Send<RequestMessage<WorkAreaViewModel?>>().Response;
        if (vm is not null)
        {
            try
            {
                vm.LayoutManager.OverwriteSaveLayout(LayoutID);

                _localizedMessageBox.Ok("Lang:MainWindow_Menu_Layout_MenuItem_LayoutList_Overwrite_SuccessMessage", "Lang:Common_MessageBoxTitle_Confirmation", vm.Title, LayoutName);
            }
            catch (Exception ex)
            {
                _localizedMessageBox.Error("Lang:MainWindow_Menu_Layout_MenuItem_LayoutList_Overwrite_FailedMessage", "Lang:Common_MessageBoxTitle_Error", ex.Message);
            }
        }
    }


    /// <summary>
    /// 編集ボタンクリック時
    /// </summary>
    [RelayCommand]
    private void OnEditButtonClicked()
    {
        var newLayoutName = MakeLayoutName(LayoutName, _localizedMessageBox);
        if (!string.IsNullOrEmpty(newLayoutName))
        {
            LayoutName = newLayoutName;
        }
    }


    /// <summary>
    /// 削除ボタンクリック時
    /// </summary>
    [RelayCommand]
    private void OnDeleteButtonClicked()
    {
        var result = _localizedMessageBox.YesNo("Lang:MainWindow_Menu_Layout_MenuItem_LayoutList_DeleteLayoutButton_ConfirmMessage", "Lang:Common_MessageBoxTitle_Confirmation", LocalizedMessageBoxResult.No, LayoutName);

        if (result == LocalizedMessageBoxResult.Yes)
        {
            SettingDatabase.Instance.Execute("DELETE FROM WorkAreaLayouts WHERE LayoutID = :LayoutID", new { LayoutID });
            Messenger.Send(new LayoutDeletedMessage(this));
        }
    }


    /// <summary>
    /// レイアウト名変更時
    /// </summary>
    partial void OnLayoutNameChanged(string value)
    {
        const string SQL = "UPDATE WorkAreaLayouts SET LayoutName = :LayoutName WHERE LayoutID = :LayoutID";

        SettingDatabase.Instance.Execute(SQL, new { LayoutName = value, LayoutID });
    }


    /// <summary>
    /// 選択状態変更時
    /// </summary>
    partial void OnIsCheckedChanged(bool value)
    {
        // プリセットが選択、または解除された場合、DB に状態を保存する
        SettingDatabase.Instance.Execute($"UPDATE WorkAreaLayouts SET IsChecked = :IsChecked WHERE LayoutID = :LayoutID", new { IsChecked = IsChecked ? 1 : 0, LayoutID });
    }


    /// <summary>
    /// 他のレイアウトの値が変更された時
    /// </summary>
    private void UncheckIfNeeded(PropertyChangedMessage<bool> message)
    {
        // プリセットが選択された場合、他のチェックを全部外す
        if (message.NewValue && this != message.Sender)
        {
            IsChecked = false;
        }
    }
}

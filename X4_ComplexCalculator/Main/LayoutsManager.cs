using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.Menu.Layout;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main;

/// <summary>
/// レイアウト管理用クラス
/// </summary>
sealed partial class LayoutsManager : ObservableRecipientEx, IRecipient<LayoutDeletedMessage>
{
    #region メンバ
    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _localizedMessageBox;
    #endregion


    #region プロパティ
    /// <summary>
    /// レイアウト一覧
    /// </summary>
    public ObservableCollection<LayoutMenuItem> Layouts { get; } = [];


    /// <summary>
    /// 現在のレイアウト
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial LayoutMenuItem? ActiveLayout { get; private set; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="workAreaManager">作業エリア管理用オブジェクト</param>
    /// <param name="localizedMessageBox">メッセージボックス表示用</param>
    public LayoutsManager(IMessenger messenger, ILocalizedMessageBox localizedMessageBox) : base(messenger, true)
    {
        _localizedMessageBox = localizedMessageBox;

        Messenger.RegisterPropertyChangedMessage(this, static (LayoutMenuItem x) => x.IsChecked, (r, m) => r.OnLayoutMenuItemIsCheckedChanged(m));
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
            Layouts.Add(new LayoutMenuItem(Messenger, _localizedMessageBox, layoutID, layoutName, isChecked));
        }

        ActiveLayout = Layouts.FirstOrDefault(x => x.IsChecked);
    }


    /// <summary>
    /// レイアウト保存
    /// </summary>
    public void SaveLayout(WorkAreaViewModel? vm)
    {
        // 計画が未選択の場合は保存対象が無いので中断
        if (vm is null)
        {
            _localizedMessageBox.Warn("Lang:MainWindow_Menu_Layout_MenuItem_SaveLayout_TabDoesNotSelectedMessage", "Lang:Common_MessageBoxTitle_Confirmation");
            return;
        }

        // ユーザにレイアウト名を入力させるが、キャンセルされたら中断
        var layoutName = LayoutMenuItem.MakeLayoutName("", _localizedMessageBox);
        if (string.IsNullOrEmpty(layoutName))
        {
            return;
        }


        try
        {
            var layoutID = vm.LayoutManager.SaveLayout(layoutName);

            Layouts.Add(new LayoutMenuItem(Messenger, _localizedMessageBox, layoutID, layoutName, false));
        }
        catch (Exception ex)
        {
            _localizedMessageBox.Error("Lang:MainWindow_Menu_Layout_MenuItem_SaveLayout_FailedMessage", "Lang:Common_MessageBoxTitle_Error", ex.Message);
        }

        _localizedMessageBox.Ok("Lang:MainWindow_Menu_Layout_MenuItem_SaveLayout_SucceededMessage", "Lang:Common_MessageBoxTitle_Confirmation", vm.Title, layoutName);
    }


    /// <summary>
    /// プリセット選択変更時
    /// </summary>
    private void OnLayoutMenuItemIsCheckedChanged(PropertyChangedMessage<bool> message)
    {
        if (message.Sender is not LayoutMenuItem item)
        {
            return;
        }

        // チェックが入ったら、選択中の項目をチェックが入った項目で更新
        if (message.NewValue)
        {
            ActiveLayout = item;
            return;
        }

        // チェックが外れた項目が現在選択中の内容なら、選択解除
        if (message.Sender == ActiveLayout)
        {
            ActiveLayout = null;
        }
    }


    /// <summary>
    /// レイアウト削除メッセージ受信
    /// </summary>
    public void Receive(LayoutDeletedMessage message)
    {
        Layouts.Remove(message.Item);

        // 選択中のレイアウトが削除されたら、選択解除
        if (ActiveLayout == message.Item)
        {
            ActiveLayout = null;
        }
    }
}

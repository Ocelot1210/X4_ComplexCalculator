using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using WPFLocalizeExtension.Engine;

namespace X4_ComplexCalculator.Common.Dialog.MessageBoxes;


/// <summary>
/// メッセージボックスの表示を行うクラス
/// </summary>
public class LocalizedMessageBoxEx : ILocalizedMessageBox
{
    /// <summary>
    /// 親ウィンドウ
    /// </summary>
    private readonly Window? _owner;


    /// <summary>
    /// OKのみのボタン
    /// </summary>
    private readonly TaskDialogButtonCollection _buttonsOkOnly;


    /// <summary>
    /// Yes / No ボタン
    /// </summary>
    private readonly TaskDialogButtonCollection _buttonsYesNo;


    /// <summary>
    /// Yes / No / Cancel ボタン
    /// </summary>
    private readonly TaskDialogButtonCollection _buttonsYesNoCancel;


    /// <summary>
    /// 新しいインスタンスを作成する
    /// </summary>
    /// <param name="owner">親ウィンドウ</param>
    public LocalizedMessageBoxEx(Window? owner)
    {
        _owner = owner;
        _buttonsOkOnly = new() { TaskDialogButton.OK };
        _buttonsYesNo = new() { TaskDialogButton.Yes, TaskDialogButton.No };
        _buttonsYesNoCancel = new() { TaskDialogButton.Yes, TaskDialogButton.No, TaskDialogButton.Cancel };
    }


    /// <inheritdoc/>
    public void Ok(string messageKey, string titleKey, params object[] vs)
    {
        ShowDialog(messageKey, titleKey, _buttonsOkOnly, TaskDialogIcon.Information, TaskDialogButton.OK, vs);
    }


    /// <inheritdoc/>
    public void Warn(string messageKey, string titleKey, params object[] vs)
    {
        ShowDialog(messageKey, titleKey, _buttonsOkOnly, TaskDialogIcon.Warning, TaskDialogButton.OK, vs);
    }


    /// <inheritdoc/>
    public void Error(string messageKey, string titleKey, params object[] vs)
    {
        ShowDialog(messageKey, titleKey, _buttonsOkOnly, TaskDialogIcon.Error, TaskDialogButton.OK, vs);
    }


    /// <inheritdoc/>
    public LocalizedMessageBoxResult YesNo(string messageKey, string titleKey, LocalizedMessageBoxResult defaultButton, params object[] vs)
    {
        return ShowDialog(messageKey, titleKey, _buttonsYesNo, TaskDialogIcon.Information, defaultButton.ToTaskDialogButton(), vs);
    }

    /// <inheritdoc/>
    public LocalizedMessageBoxResult YesNoCancel(string messageKey, string titleKey, LocalizedMessageBoxResult defaultButton, params object[] vs)
    {
        return ShowDialog(messageKey, titleKey, _buttonsYesNoCancel, TaskDialogIcon.Information, defaultButton.ToTaskDialogButton(), vs);
    }

    /// <inheritdoc/>
    public LocalizedMessageBoxResult YesNoWarn(string messageKey, string titleKey, LocalizedMessageBoxResult defaultButton, params object[] vs)
    {
        return ShowDialog(messageKey, titleKey, _buttonsYesNo, TaskDialogIcon.Warning, defaultButton.ToTaskDialogButton(), vs);
    }


    /// <summary>
    /// メッセージボックスを表示する
    /// </summary>
    /// <param name="messageKey">表示文字列用キー</param>
    /// <param name="titleKey">タイトル部分用キー</param>
    /// <param name="buttons">表示するボタン</param>
    /// <param name="icon">表示するアイコン</param>
    /// <param name="defaultButton">初期選択状態のボタン</param>
    /// <param name="param"><paramref name="messageKey"/>用のパラメータ</param>
    /// <returns>選択されたボタンを表す <see cref="TaskDialogResult"/></returns>
    private LocalizedMessageBoxResult ShowDialog(
        string messageKey,
        string titleKey,
        TaskDialogButtonCollection buttons,
        TaskDialogIcon icon,
        TaskDialogButton defaultButton,
        object[] param
    )
    {
        if (_owner is null)
        {
            return ShowDialogMain(IntPtr.Zero, messageKey, titleKey, buttons, icon, defaultButton, param);
        }
        else
        {
            return _owner.Dispatcher.Invoke(() =>
            {
                var hwndOwner = new WindowInteropHelper(_owner).Handle;

                return ShowDialogMain(hwndOwner, messageKey, titleKey, buttons, icon, defaultButton, param);
            });
        }
    }

    /// <summary>
    /// メッセージボックスを表示する
    /// </summary>
    /// <param name="hwndOwner">親ウィンドウ</param>
    /// <param name="messageKey">表示文字列用キー</param>
    /// <param name="titleKey">タイトル部分用キー</param>
    /// <param name="buttons">表示するボタン</param>
    /// <param name="icon">表示するアイコン</param>
    /// <param name="defaultButton">初期選択状態のボタン</param>
    /// <param name="param"><paramref name="messageKey"/>用のパラメータ</param>
    /// <returns>選択されたボタンを表す <see cref="TaskDialogResult"/></returns>
    private static LocalizedMessageBoxResult ShowDialogMain(
        IntPtr hwndOwner,
        string messageKey,
        string titleKey,
        TaskDialogButtonCollection buttons,
        TaskDialogIcon icon,
        TaskDialogButton defaultButton,
        object[] param
    )
    {
        var page = new TaskDialogPage()
        {
            Text = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject(messageKey, null, null), param),
            Caption = (string)LocalizeDictionary.Instance.GetLocalizedObject(titleKey, null, null),
            Icon = icon,
            Buttons = buttons,
            DefaultButton = defaultButton,
        };

        return TaskDialog.ShowDialog(hwndOwner, page).ToLocalizedMessageBoxResult();
    }
}

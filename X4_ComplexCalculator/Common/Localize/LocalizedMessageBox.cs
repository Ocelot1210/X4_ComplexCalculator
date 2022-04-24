using System;
using System.Windows;

namespace X4_ComplexCalculator.Common.Localize;

/// <summary>
/// ローカライズ対応メッセージボックス
/// </summary>
public class LocalizedMessageBox
{
    /// <summary>
    /// フック処理
    /// </summary>
    private static Win32.WindowsHookProc? _HookProcDelegate = null;

    /// <summary>
    /// フックプロシージャのハンドル
    /// </summary>
    private static int _hHook = 0;

    /// <summary>
    /// メッセージボックスのタイトル文字列
    /// </summary>
    private static string? _Title = null;

    /// <summary>
    /// メッセージボックスの表示文字列
    /// </summary>
    private static string? _Msg = null;



    /// <summary>
    /// ローカライズされたメッセージボックスを表示
    /// </summary>
    /// <param name="messageBoxTextKey">表示文字列用キー</param>
    /// <param name="captionKey">タイトル部分用キー</param>
    /// <param name="button">ボタンのスタイル</param>
    /// <param name="icon">アイコンのスタイル</param>
    /// <param name="defaultResult">フォーカスするボタン</param>
    /// <param name="param">表示文字列用パラメータ</param>
    /// <returns>MessageBox.Showの戻り値</returns>
    public static MessageBoxResult Show(
        string              messageBoxTextKey, 
        string              captionKey      = "",
        MessageBoxButton    button          = MessageBoxButton.OK,
        MessageBoxImage     icon            = MessageBoxImage.None,
        MessageBoxResult    defaultResult   = MessageBoxResult.OK,
        params object[] param)
    {
        var format = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject(messageBoxTextKey, null, null);

        _Msg = string.Format(format, param);
        _Title = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject(captionKey, null, null); ;

        _HookProcDelegate = new Win32.WindowsHookProc(HookCallback);

        _hHook = Win32.SetWindowsHookEx(Win32.WH_CBT, _HookProcDelegate, IntPtr.Zero, Win32.GetCurrentThreadId());

        var result = MessageBox.Show(_Msg, _Title, button, icon, defaultResult);

        UnHook();

        return result;
    }


    /// <summary>
    /// フック時のコールバック処理
    /// </summary>
    /// <param name="code"></param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <returns></returns>
    private static int HookCallback(int code, IntPtr wParam, IntPtr lParam)
    {
        int hHook = _hHook;

        if (code == Win32.HCBT_ACTIVATE)
        {
            // ハンドルのクラス名がダイアログボックスの場合のみ処理
            if (Win32.GetClassName(wParam) == "#32770")
            {
                // ダイアログボックスのタイトルとメッセージを取得
                string title = Win32.GetWindowText(wParam);
                string msg = Win32.GetDlgItemText(wParam, 0xFFFF);      // -1 = IDC_STATIC

                // タイトルとメッセージが一致した場合、ダイアログの位置を親ウィンドウの中央に移動する
                if ((title == _Title) && (msg == _Msg))
                {
                    MoveToCenterOnParent(wParam);
                    UnHook();
                }
            }
        }

        return Win32.CallNextHookEx(hHook, code, wParam, lParam);
    }


    /// <summary>
    /// フック解除
    /// </summary>
    private static void UnHook()
    {
        Win32.UnhookWindowsHookEx(_hHook);
        _hHook = 0;
        _Msg   = null;
        _Title = null;
        _HookProcDelegate = null;
    }


    /// <summary>
    /// 子ウィンドウを親ウィンドウの中央に移動する
    /// </summary>
    /// <param name="hChildWnd"></param>
    private static void MoveToCenterOnParent(IntPtr hChildWnd)
    {
        // 子ウィンドウの領域取得
        var childRect = new Win32.RECT();
        Win32.GetWindowRect(hChildWnd, ref childRect);
        int cxChild = childRect.Right  - childRect.Left;
        int cyChild = childRect.Bottom - childRect.Top;

        // 親ウィンドウの領域取得
        var parentRect = new Win32.RECT();
        Win32.GetWindowRect(Win32.GetParent(hChildWnd), ref parentRect);
        int cxParent = parentRect.Right  - parentRect.Left;
        int cyParent = parentRect.Bottom - parentRect.Top;

        // 子ウィンドウを親ウィンドウの中央に移動
        int x = parentRect.Left + (cxParent - cxChild) / 2;
        int y = parentRect.Top  + (cyParent - cyChild) / 2;
        Win32.SetWindowPos(hChildWnd, IntPtr.Zero, x, y, 0, 0, 0x15);   // 0x15 = SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE
    }
}

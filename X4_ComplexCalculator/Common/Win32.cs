using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace X4_ComplexCalculator.Common
{
    /// <summary>
    /// Win32API用ラッパクラス
    /// </summary>
    public class Win32
    {
        #region 定数
        public const int WH_CBT = 5;
        public const int HCBT_ACTIVATE = 5;
        #endregion

        #region 構造体
        /// <summary>
        /// 矩形情報構造体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        #endregion

        #region 関数
        /// <summary>
        /// SetWindowHook()用関数ポインタ
        /// </summary>
        public delegate int WindowsHookProc(int nCode, IntPtr wParam, IntPtr lParam);


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int SetWindowsHookEx(int idHook, WindowsHookProc lpfn, IntPtr hInstance, int threadId);


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);


        [DllImport("user32.dll")]
        public static extern uint GetDlgItemText(IntPtr hDlg, int nIDDlgItem, [Out] StringBuilder lpString, int nMaxCount);


        [DllImport("user32.dll")]
        public static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);


        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);


        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr handle, ref RECT r);


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);


        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();
        #endregion


        #region 処理簡略化のためのメソッド

        /// <summary>
        /// 指定したハンドルのクラス名を取得
        /// </summary>
        /// <param name="hWnd">対象ハンドル</param>
        /// <returns>クラス名</returns>
        public static string GetClassName(IntPtr hWnd)
        {
            var className = new StringBuilder(128);

            GetClassName(hWnd, className, className.Capacity);
            return className.ToString();
        }


        /// <summary>
        /// 指定したウィンドウハンドルのタイトル文字列を取得s
        /// </summary>
        /// <param name="hWnd">対象ウィンドウハンドル</param>
        /// <returns>タイトル文字列</returns>
        public static string GetWindowText(IntPtr hWnd)
        {
            int len = GetWindowTextLength(hWnd);

            var sb = new StringBuilder(len + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }


        /// <summary>
        /// 指定したダイアログの指定したコントロールIDの文字列を取得する
        /// </summary>
        /// <param name="hDlg">対象ダイアログハンドル</param>
        /// <param name="nIDDlgItem">対象コントロールID</param>
        /// <returns>コントロールの文字列</returns>
        public static string GetDlgItemText(IntPtr hDlg, int nIDDlgItem)
        {
            IntPtr hItem = GetDlgItem(hDlg, nIDDlgItem);
            if (hItem == IntPtr.Zero)
            {
                return "";
            }

            int len = GetWindowTextLength(hItem);
            var sb = new StringBuilder(len + 1);
            GetWindowText(hItem, sb, sb.Capacity);
            return sb.ToString();
        }
        #endregion
    }
}

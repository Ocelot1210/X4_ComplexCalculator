using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;

namespace X4_ComplexCalculator.Common.Dialog.MessageBoxes;


/// <summary>
/// メッセージボックスの表示を行うクラスの抽象 Interface
/// </summary>
public interface ILocalizedMessageBox
{
    /// <summary>
    /// OK ボタンのみのメッセージボックスを表示する (Information マーク)
    /// </summary>
    /// <param name="messageKey">表示文字列用キー</param>
    /// <param name="titleKey">タイトル部分用キー</param>
    /// <param name="vs"><paramref name="messageKey"/>用のパラメータ</param>
    void Ok(string messageKey, string titleKey, params object[] vs);


    /// <summary>
    /// OK ボタンのみのメッセージボックスを表示する (Warning マーク)
    /// </summary>
    /// <param name="messageKey">表示文字列用キー</param>
    /// <param name="titleKey">タイトル部分用キー</param>
    /// <param name="vs"><paramref name="messageKey"/>用のパラメータ</param>
    void Warn(string messageKey, string titleKey, params object[] vs);


    /// <summary>
    /// エラー用のメッセージボックスを表示する
    /// </summary>
    /// <param name="messageKey">表示文字列用キー</param>
    /// <param name="titleKey">タイトル部分用キー</param>
    /// <param name="vs"><paramref name="messageKey"/>用のパラメータ</param>
    void Error(string messageKey, string titleKey, params object[] vs);


    /// <summary>
    /// Yes / No ボタンのメッセージボックスを表示する (Information マーク)
    /// </summary>
    /// <param name="messageKey">表示文字列用キー</param>
    /// <param name="titleKey">タイトル部分用キー</param>
    /// <param name="defaultButton">デフォルトのボタン</param>
    /// <param name="vs"><paramref name="messageKey"/>用のパラメータ</param>
    /// <returns>選択されたボタンを表す <see cref="TaskDialogResult"/></returns>
    LocalizedMessageBoxResult YesNo(string messageKey, string titleKey, LocalizedMessageBoxResult defaultButton, params object[] vs);


    /// <summary>
    /// Yes / No / Cancel ボタンのメッセージボックスを表示する (Information マーク)
    /// </summary>
    /// <param name="messageKey">表示文字列用キー</param>
    /// <param name="titleKey">タイトル部分用キー</param>
    /// <param name="defaultButton">デフォルトのボタン</param>
    /// <param name="vs"><paramref name="messageKey"/>用のパラメータ</param>
    /// <returns>選択されたボタンを表す <see cref="TaskDialogResult"/></returns>
    LocalizedMessageBoxResult YesNoCancel(string messageKey, string titleKey, LocalizedMessageBoxResult defaultButton, params object[] vs);


    /// <summary>
    /// Yes / No ボタンのメッセージボックスを表示する (警告マーク)
    /// </summary>
    /// <param name="messageKey">表示文字列用キー</param>
    /// <param name="titleKey">タイトル部分用キー</param>
    /// <param name="defaultButton">デフォルトのボタン</param>
    /// <param name="vs"><paramref name="messageKey"/>用のパラメータ</param>
    /// <returns>選択されたボタンを表す <see cref="TaskDialogResult"/></returns>
    LocalizedMessageBoxResult YesNoWarn(string messageKey, string titleKey, LocalizedMessageBoxResult defaultButton, params object[] vs);


    /// <summary>
    /// 親ウィンドウを設定する
    /// </summary>
    /// <param name="window">新しいウィンドウ</param>
    void SetOwner(Window? window);


    /// <summary>
    /// 自分自身を複製する
    /// </summary>
    /// <returns>複製結果</returns>
    ILocalizedMessageBox Clone();
}
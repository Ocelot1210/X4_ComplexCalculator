using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace X4_ComplexCalculator.Common.Dialogs.SelectStringDialog;

/// <summary>
/// 文字列選択ダイアログのModel
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="initialString">初期文字列</param>
/// <param name="isValidInput">文字列が有効か判定する関数</param>
sealed partial class SelectStringDialogModel(string initialString, Predicate<string>? isValidInput) : ObservableObject
{
    #region メンバ
    /// <summary>
    /// 入力が有効か判定する関数
    /// </summary>
    private readonly Predicate<string>? _isValidInput = isValidInput;
    #endregion


    #region プロパティ
    /// <summary>
    /// ダイアログの戻り値
    /// </summary>
    [ObservableProperty]
    public partial bool DialogResult { get; private set; }


    /// <summary>
    /// ダイアログを閉じるか
    /// </summary>
    [ObservableProperty]
    public partial bool CloseDialogProperty { get; private set; }


    /// <summary>
    /// 入力文字列
    /// </summary>
    [ObservableProperty]
    public partial string InputString { get; set; } = initialString;
    #endregion


    /// <summary>
    /// OKボタンクリック時の処理
    /// </summary>
    [RelayCommand]
    private void OnOkButtonClicked()
    {
        // 入力が有効ならダイアログを閉じる
        if (_isValidInput?.Invoke(InputString) ?? true)
        {
            DialogResult = true;
            CloseDialogProperty = true;
        }
    }


    /// <summary>
    /// キャンセルボタンクリック時の処理
    /// </summary>
    [RelayCommand]
    private void OnCancelButtonClicked() => CloseDialogProperty = true;
}

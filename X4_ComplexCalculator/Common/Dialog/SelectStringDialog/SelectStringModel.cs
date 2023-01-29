using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows.Input;

namespace X4_ComplexCalculator.Common.Dialog.SelectStringDialog;

/// <summary>
/// 文字列選択ダイアログのModel
/// </summary>
class SelectStringDialogModel : BindableBase
{
    #region メンバ
    /// <summary>
    /// ダイアログの戻り値
    /// </summary>
    private bool _dialogResult;


    /// <summary>
    /// ダイアログを閉じるか
    /// </summary>
    private bool _closeDialogProperty;


    /// <summary>
    /// 入力文字列
    /// </summary>
    private string _inputString = "";


    /// <summary>
    /// 入力が有効か判定する関数
    /// </summary>
    private readonly Predicate<string>? _isValidInput;
    #endregion


    #region プロパティ
    /// <summary>
    /// ダイアログの戻り値
    /// </summary>
    public bool DialogResult
    {
        get => _dialogResult;
        set => SetProperty(ref _dialogResult, value);
    }


    /// <summary>
    /// ダイアログを閉じるか
    /// </summary>
    public bool CloseDialogProperty
    {
        get => _closeDialogProperty;
        set => SetProperty(ref _closeDialogProperty, value);
    }


    /// <summary>
    /// 入力文字列
    /// </summary>
    public string InputString
    {
        get => _inputString;
        set => SetProperty(ref _inputString, value);
    }


    /// <summary>
    /// OKボタンクリック時の処理
    /// </summary>
    public ICommand OkButtonClickedCommand { get; }


    /// <summary>
    /// キャンセルボタンクリック時の処理
    /// </summary>
    public ICommand CancelButtonClickedCommand { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="initialString">初期文字列</param>
    /// <param name="isValidInput">文字列が有効か判定する関数</param>
    public SelectStringDialogModel(string initialString, Predicate<string>? isValidInput)
    {
        InputString = initialString;
        _isValidInput = isValidInput;
        OkButtonClickedCommand = new DelegateCommand(OnOkButtonClick);
        CancelButtonClickedCommand = new DelegateCommand(OnCancelButtonClicked);
    }


    /// <summary>
    /// OKボタンクリック時の処理
    /// </summary>
    private void OnOkButtonClick()
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
    private void OnCancelButtonClicked() => CloseDialogProperty = true;
}

using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.ModulesGrid.EditEquipment.EditPresetName
{
    /// <summary>
    /// プリセット名編集用ViewModel
    /// </summary>
    class EditPresetNameViewModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// Model
        /// </summary>
        private readonly EditPresetNameModel _Model;

        /// <summary>
        /// ダイアログの戻り値
        /// </summary>
        private bool _DialogResult;

        /// <summary>
        /// ダイアログを閉じるか
        /// </summary>
        private bool _CloseDialogProperty;
        #endregion


        #region プロパティ
        /// <summary>
        /// ダイアログの戻り値
        /// </summary>
        public bool DialogResult
        {
            get
            {
                return _DialogResult;
            }
            set
            {
                SetProperty(ref _DialogResult, value);
            }
        }


        /// <summary>
        /// ダイアログを閉じるか
        /// </summary>
        public bool CloseDialogProperty
        {
            get
            {
                return _CloseDialogProperty;
            }
            set
            {
                SetProperty(ref _CloseDialogProperty, value);
            }
        }


        /// <summary>
        /// 変更前プリセット名
        /// </summary>
        public string OriginalPresetName => _Model.OrigPresetName;

        
        /// <summary>
        /// 変更後プリセット名
        /// </summary>
        public string NewPresetName
        {
            set
            {
                _Model.NewPresetName = value;
            }
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
        /// <param name="origModuleName">変更前プリセット名</param>
        public EditPresetNameViewModel(string origModuleName)
        {
            _Model = new EditPresetNameModel(origModuleName);
            OkButtonClickedCommand = new DelegateCommand(OnOkButtonClick);
            CancelButtonClickedCommand = new DelegateCommand(OnCancelButtonClicked);
        }


        /// <summary>
        /// OKボタンクリック時の処理
        /// </summary>
        private void OnOkButtonClick()
        {
            // プリセット名が有効か
            if (_Model.IsValidPresetName)
            {
                DialogResult = true;
                CloseDialogProperty = true;
            }
            else
            {
                MessageBox.Show("プリセット名が無効です。\r\nプリセット名は空白文字以外の文字が1文字以上必要です。", "確認", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        /// <summary>
        /// キャンセルボタンクリック時の処理
        /// </summary>
        private void OnCancelButtonClicked()
        {
            CloseDialogProperty = true;
        }
    }
}

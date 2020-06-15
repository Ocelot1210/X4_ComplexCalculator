using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Input;
using X4_ComplexCalculator.Common.Dialog.SelectStringDialog;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB;

namespace X4_ComplexCalculator.Main.Menu.Layout
{
    /// <summary>
    /// レイアウト一覧の1レコード分
    /// </summary>
    public class LayoutMenuItem : BindableBase, IComparable
    {
        #region メンバ
        /// <summary>
        /// レイアウト名
        /// </summary>
        private string _LayoutName = "";

        /// <summary>
        /// チェック状態
        /// </summary>
        private bool _IsChecked = false;

        /// <summary>
        /// 削除されたか
        /// </summary>
        private bool _IsDeleted = false;
        #endregion

        #region プロパティ
        /// <summary>
        /// レイアウトID
        /// </summary>
        public long LayoutID;

        /// <summary>
        /// レイアウト名
        /// </summary>
        public string LayoutName
        {
            get => _LayoutName;
            set => SetProperty(ref _LayoutName, value);
        }

        /// <summary>
        /// 保存ボタンクリック時
        /// </summary>
        public ICommand SaveButtonClickedCommand { get; }


        /// <summary>
        /// 編集ボタンクリック時
        /// </summary>
        public ICommand EditButtonClickedCommand { get; }


        /// <summary>
        /// 削除ボタンクリック時
        /// </summary>
        public ICommand DeleteButtonClickedCommand { get; }


        /// <summary>
        /// チェック状態
        /// </summary>
        public bool IsChecked
        {
            get => _IsChecked;
            set
            {
                if (SetProperty(ref _IsChecked, value))
                {
                    DBConnection.CommonDB.ExecQuery($"UPDATE WorkAreaLayouts SET IsChecked = {(IsChecked? 1 : 0)} WHERE LayoutID = {LayoutID}");
                }
            }
        }

        /// <summary>
        /// 削除されたか
        /// </summary>
        public bool IsDeleted
        {
            get => _IsDeleted;
            set => SetProperty(ref _IsDeleted, value);
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layoutID">レイアウトID</param>
        /// <param name="layoutName">レイアウト名</param>
        /// <param name="isChecked">チェックされているか</param>
        public LayoutMenuItem(long layoutID, string layoutName, bool isChecked)
        {
            LayoutID = layoutID;
            LayoutName = layoutName;
            IsChecked = isChecked;
            SaveButtonClickedCommand    = new DelegateCommand(SaveLayout);
            EditButtonClickedCommand    = new DelegateCommand(EditLayoutName);
            DeleteButtonClickedCommand  = new DelegateCommand(DeleteLayout);
        }


        /// <summary>
        /// レイアウトを上書き保存
        /// </summary>
        private void SaveLayout()
        {
            // このクラス内では現在のレイアウトが分からないのでPropertyChangedイベントを発火させて
            // MainWindowModelにてレイアウトを上書きする
            // → 何かいい方法が思いついたら直す
            RaisePropertyChanged(nameof(SaveButtonClickedCommand));
        }


        /// <summary>
        /// レイアウト名変更
        /// </summary>
        private void EditLayoutName()
        {
            var (onOK, newLayoutName) = SelectStringDialog.ShowDialog("Lang:EditLayoutName", "Lang:LayoutName", LayoutName, IsValidLayoutName);
            if (onOK && LayoutName != newLayoutName)
            {
                LayoutName = newLayoutName;

                var param = new SQLiteCommandParameters(2);
                param.Add("layoutName", System.Data.DbType.String, LayoutName);
                param.Add("layoutID", System.Data.DbType.Int32, LayoutID);
                DBConnection.CommonDB.ExecQuery($"UPDATE WorkAreaLayouts SET LayoutName = :layoutName WHERE LayoutID = :layoutID", param);
            }
        }


        /// <summary>
        /// レイアウト削除
        /// </summary>
        private void DeleteLayout()
        {
            var result = LocalizedMessageBox.Show("Lang:DeleteLayoutConfirmMessage", "Lang:Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No, LayoutName);

            if (result == MessageBoxResult.Yes)
            {
                DBConnection.CommonDB.ExecQuery($"DELETE FROM WorkAreaLayouts WHERE LayoutID = {LayoutID}");
                IsDeleted = true;
            }
        }


        /// <summary>
        /// レイアウト名が有効か判定
        /// </summary>
        /// <param name="layoutName">レイアウト名</param>
        /// <returns>レイアウト名が有効か</returns>
        public static bool IsValidLayoutName(string layoutName)
        {
            var ret = true;

            if (string.IsNullOrWhiteSpace(layoutName))
            {
                LocalizedMessageBox.Show("Lang:InvalidLayoutNameMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                ret = false;
            }

            return ret;
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is LayoutMenuItem item))
            {
                throw new InvalidOperationException();
            }

            return (item.LayoutID < LayoutID)?  1 :
                   (item.LayoutID > LayoutID)? -1 : 0;
        }
    }
}

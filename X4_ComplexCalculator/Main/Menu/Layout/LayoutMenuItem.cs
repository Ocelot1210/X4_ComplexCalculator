using System;
using Prism.Mvvm;
using Reactive.Bindings;

namespace X4_ComplexCalculator.Main.Menu.Layout
{
    /// <summary>
    /// レイアウト一覧の1レコード分
    /// </summary>
    public class LayoutMenuItem : BindableBase, IComparable
    {
        #region プロパティ
        /// <summary>
        /// レイアウトID
        /// </summary>
        public long LayoutID;


        /// <summary>
        /// レイアウト名
        /// </summary>
        public ReactivePropertySlim<string> LayoutName { get; }


        /// <summary>
        /// 保存ボタンクリック時
        /// </summary>
        public ReactiveCommand SaveButtonClickedCommand { get; } = new ReactiveCommand();


        /// <summary>
        /// 編集ボタンクリック時
        /// </summary>
        public ReactiveCommand EditButtonClickedCommand { get; } = new ReactiveCommand();


        /// <summary>
        /// 削除ボタンクリック時
        /// </summary>
        public ReactiveCommand DeleteButtonClickedCommand { get; } = new ReactiveCommand();


        /// <summary>
        /// チェック状態
        /// </summary>
        public ReactivePropertySlim<bool> IsChecked { get; }
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
            LayoutName = new ReactivePropertySlim<string>(layoutName);
            IsChecked = new ReactivePropertySlim<bool>(isChecked);
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

            return (item.LayoutID < LayoutID) ?  1 :
                   (item.LayoutID > LayoutID) ? -1 : 0;
        }
    }
}

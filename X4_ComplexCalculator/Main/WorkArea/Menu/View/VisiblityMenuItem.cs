﻿using System;
using X4_ComplexCalculator.Common;
using Xceed.Wpf.AvalonDock.Layout;
using System.ComponentModel;

namespace X4_ComplexCalculator.Main.WorkArea.Menu.View
{
    /// <summary>
    /// 表示状態メニュー1つ分
    /// </summary>
    class VisiblityMenuItem : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 表示/非表示設定対象
        /// </summary>
        private LayoutAnchorable _LayoutAnchorable;

        /// <summary>
        /// 表示状態変更通知をすべきか
        /// </summary>
        private bool _ShouldNotifyVisibiltyChange;
        #endregion


        /// <summary>
        /// タイトル文字列
        /// </summary>
        public string Title => _LayoutAnchorable.Title;


        /// <summary>
        /// チェック状態
        /// </summary>
        public bool IsChecked
        {
            get => _LayoutAnchorable.IsVisible;
            set
            {
                if (_LayoutAnchorable.IsVisible != value)
                {
                    _ShouldNotifyVisibiltyChange = false;
                    _LayoutAnchorable.IsVisible  = value;
                    _ShouldNotifyVisibiltyChange = true;

                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layoutElement">表示/非表示設定対象</param>
        public VisiblityMenuItem(LayoutAnchorable layoutElement)
        {
            _LayoutAnchorable = layoutElement;
            _LayoutAnchorable.IsVisibleChanged += LayoutAnchorable_IsVisibleChanged;
            _ShouldNotifyVisibiltyChange = true;
        }


        /// <summary>
        /// 表示/非表示状態変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayoutAnchorable_IsVisibleChanged(object sender, EventArgs e)
        {
            if (_ShouldNotifyVisibiltyChange)
            {
                OnPropertyChanged(nameof(IsChecked));
            }
        }
    }
}
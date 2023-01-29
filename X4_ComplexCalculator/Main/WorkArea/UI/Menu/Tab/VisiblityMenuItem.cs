using AvalonDock.Layout;
using Prism.Mvvm;
using System;

namespace X4_ComplexCalculator.Main.WorkArea.UI.Menu.Tab;

/// <summary>
/// 表示状態メニュー1レコード分のクラス
/// </summary>
public class VisiblityMenuItem : BindableBase
{
    #region メンバ
    /// <summary>
    /// 表示/非表示設定対象
    /// </summary>
    private readonly LayoutAnchorable _layoutAnchorable;


    /// <summary>
    /// 表示状態変更通知をすべきか
    /// </summary>
    private bool _shouldNotifyVisibiltyChange;
    #endregion


    #region プロパティ
    /// <summary>
    /// タイトル文字列
    /// </summary>
    public string Title => _layoutAnchorable.Title;


    /// <summary>
    /// チェック状態
    /// </summary>
    public bool IsChecked
    {
        get => _layoutAnchorable.IsVisible;
        set
        {
            if (_layoutAnchorable.IsVisible != value)
            {
                _shouldNotifyVisibiltyChange = false;
                _layoutAnchorable.IsVisible  = value;
                _shouldNotifyVisibiltyChange = true;

                RaisePropertyChanged();
            }
        }
    }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="layoutElement">表示/非表示設定対象</param>
    public VisiblityMenuItem(LayoutAnchorable layoutElement)
    {
        _layoutAnchorable = layoutElement;
        _layoutAnchorable.IsVisibleChanged += LayoutAnchorable_IsVisibleChanged;
        _shouldNotifyVisibiltyChange = true;
    }


    /// <summary>
    /// 表示/非表示状態変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LayoutAnchorable_IsVisibleChanged(object? sender, EventArgs e)
    {
        if (_shouldNotifyVisibiltyChange)
        {
            RaisePropertyChanged(nameof(IsChecked));
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main.Menu.View.EmpireOverviews;


/// <summary>
/// 帝国の概要用の計画一覧
/// </summary>
public sealed partial class WorkAreaItem : ObservableRecipientEx
{
    /// <summary>
    /// 計画名
    /// </summary>
    public string Title => WorkArea.Title;


    /// <summary>
    /// 計画
    /// </summary>
    public WorkAreaViewModel WorkArea { get; }


    /// <summary>
    /// 集計対象か
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial bool IsChecked { get; set; }


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="workArea">計画</param>
    /// <param name="isChecked">集計対象か</param>
    public WorkAreaItem(IMessenger messenger, WorkAreaViewModel workArea, bool isChecked) : base(messenger)
    {
        WorkArea = workArea;
        IsChecked = isChecked;

        IsActive = true;
    }
}
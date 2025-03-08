using CommunityToolkit.Mvvm.ComponentModel;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main.Menu.View.EmpireOverviews;

/// <summary>
/// コンストラクタ
/// </summary>
/// <param name="workArea">計画</param>
/// <param name="isChecked">集計対象か</param>
public sealed partial class WorkAreaItem(WorkAreaViewModel workArea, bool isChecked) : ObservableObject
{
    /// <summary>
    /// 集計対象か
    /// </summary>
    [ObservableProperty]
    public partial bool IsChecked { get; set; } = isChecked;


    /// <summary>
    /// 計画
    /// </summary>
    public WorkAreaViewModel WorkArea { get; } = workArea;


    /// <summary>
    /// 計画名
    /// </summary>
    public string Title => WorkArea.Title;
}
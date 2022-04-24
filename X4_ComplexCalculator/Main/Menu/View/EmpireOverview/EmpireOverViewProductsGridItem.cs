using Prism.Mvvm;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.Menu.View.EmpireOverview;

/// <summary>
/// 帝国の概要の製品一覧DataGridの1レコード分
/// </summary>
public class EmpireOverViewProductsGridItem : BindableBase
{
    #region メンバ
    /// <summary>
    /// 生産量
    /// </summary>
    private long _Count;
    #endregion


    #region プロパティ
    /// <summary>
    /// ウェア
    /// </summary>
    public IWare Ware { get; }


    /// <summary>
    /// 生産量
    /// </summary>
    public long Count
    {
        get => _Count;
        set => SetProperty(ref _Count, value);
    }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="ware">表示対象ウェア</param>
    /// <param name="count">生産量</param>
    public EmpireOverViewProductsGridItem(IWare ware, long count)
    {
        Ware = ware;
        Count = count;
    }
}

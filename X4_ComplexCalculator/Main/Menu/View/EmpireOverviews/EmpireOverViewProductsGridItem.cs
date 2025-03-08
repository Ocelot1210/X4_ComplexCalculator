using CommunityToolkit.Mvvm.ComponentModel;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.Menu.View.EmpireOverviews;

/// <summary>
/// 帝国の概要の製品一覧DataGridの1レコード分
/// </summary>
public sealed partial class EmpireOverViewProductsGridItem : ObservableObject
{
    #region プロパティ
    /// <summary>
    /// ウェア
    /// </summary>
    public IWare Ware { get; }


    /// <summary>
    /// 余剰生産量
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Count))]
    public partial long Surplus { get; set; }


    /// <summary>
    /// 不足生産量
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Count))]
    public partial long Shortage { get; set; }


    /// <summary>
    /// 総生産数
    /// </summary>
    public long Count => Surplus - Shortage;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="ware">表示対象ウェア</param>
    /// <param name="amount">生産量</param>
    public EmpireOverViewProductsGridItem(IWare ware, long amount)
    {
        Ware = ware;
        AddProduct(amount);
    }


    /// <summary>
    /// 製品が追加された時
    /// </summary>
    /// <param name="amount">追加された製品の生産量/消費量</param>
    public void AddProduct(long amount)
    {
        if (amount == 0) return;

        if (0 < amount)
        {
            Surplus += amount;
        }
        else
        {
            Shortage += (-amount);
        }
    }


    /// <summary>
    /// 製品が削除された時
    /// </summary>
    /// <param name="amount">削除された製品の生産量/消費量</param>
    public void DeleteProduct(long amount)
    {
        if (amount == 0) return;

        if (0 < amount)
        {
            Surplus -= amount;
        }
        else
        {
            Shortage -= (-amount);
        }
    }


    /// <summary>
    /// 製品の生産量/消費量が更新された時
    /// </summary>
    /// <param name="oldAmount">生産量/消費量の更新前の値</param>
    /// <param name="newAmount">生産量/消費量の更新後の値</param>
    public void UpdateProduct(long oldAmount, long newAmount)
    {
        if (0 < oldAmount)
        {
            Surplus -= oldAmount;
        }
        else
        {
            Shortage -= (-oldAmount);
        }

        if (0 < newAmount)
        {
            Surplus += newAmount;
        }
        else
        {
            Shortage += (-newAmount);
        }
    }
}

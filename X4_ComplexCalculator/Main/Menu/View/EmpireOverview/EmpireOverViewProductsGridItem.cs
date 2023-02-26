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
    /// 余剰生産量
    /// </summary>
    private long _surplus;


    /// <summary>
    /// 不足生産量
    /// </summary>
    private long _shortage;
    #endregion


    #region プロパティ
    /// <summary>
    /// ウェア
    /// </summary>
    public IWare Ware { get; }


    /// <summary>
    /// 余剰生産量
    /// </summary>
    public long Surplus
    {
        get => _surplus;
        set => SetProperty(ref _surplus, value);
    }


    /// <summary>
    /// 不足生産量
    /// </summary>
    public long Shortage
    {
        get => _shortage;
        set => SetProperty(ref _shortage, value);
    }


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

        RaisePropertyChanged(nameof(Count));
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

        RaisePropertyChanged(nameof(Count));
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
            _surplus -= oldAmount;
        }
        else
        {
            _shortage -= (-oldAmount);
        }

        if (0 < newAmount)
        {
            _surplus += newAmount;
        }
        else
        {
            _shortage += (-newAmount);
        }

        RaisePropertyChanged(nameof(Surplus));
        RaisePropertyChanged(nameof(Shortage));
        RaisePropertyChanged(nameof(Count));
    }
}

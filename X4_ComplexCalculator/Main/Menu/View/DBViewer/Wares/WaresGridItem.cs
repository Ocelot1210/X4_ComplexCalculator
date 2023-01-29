using Prism.Mvvm;
using System;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.Menu.View.DBViewer.Wares;

/// <summary>
/// ウェア閲覧用DataGridの1レコード分
/// </summary>
class WaresGridItem : BindableBase
{
    #region メンバ
    /// <summary>
    /// 表示対象ウェア
    /// </summary>
    private readonly IWare _ware;
    #endregion


    #region プロパティ
    /// <summary>
    /// ウェア名
    /// </summary>
    public string WareName => _ware.Name;


    /// <summary>
    /// ウェア種別名
    /// </summary>
    public string WareGroupName => _ware.WareGroup.Name;


    /// <summary>
    /// カーゴ種別名
    /// </summary>
    public string Transport => _ware.TransportType.Name;


    /// <summary>
    /// 最安値
    /// </summary>
    public long MinPrice => _ware.MinPrice;


    /// <summary>
    /// 最高値
    /// </summary>
    public long MaxPrice => _ware.MaxPrice;


    /// <summary>
    /// 利益
    /// </summary>
    public long Profit => MaxPrice - MinPrice;


    /// <summary>
    /// 容量
    /// </summary>
    public long Volume => _ware.Volume;


    /// <summary>
    /// 容量当たりの利益
    /// </summary>
    public double ProfitPreVolume => Math.Round((double)Profit / Volume, 1);
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="ware">表示対象ウェア</param>
    public WaresGridItem(IWare ware)
    {
        _ware = ware;
    }
}

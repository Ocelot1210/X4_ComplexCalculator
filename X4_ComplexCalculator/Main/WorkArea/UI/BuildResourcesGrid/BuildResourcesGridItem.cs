using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;

/// <summary>
/// 建造に必要なウェアを表示するDataGridViewの1レコード分のクラス
/// </summary>
public class BuildResourcesGridItem : BindableBaseEx, IEditable, ISelectable
{
    #region メンバ
    /// <summary>
    /// 単価
    /// </summary>
    private long _unitPrice;


    /// <summary>
    /// 建造に必要なウェア数量
    /// </summary>
    private long _amount;


    /// <summary>
    /// 建造ウェアを購入しない
    /// </summary>
    private bool _noBuy;


    /// <summary>
    /// 編集状態
    /// </summary>
    private EditStatus _editStatus = EditStatus.Unedited;
    #endregion


    #region プロパティ
    /// <summary>
    /// 建造に必要なウェア
    /// </summary>
    public IWare Ware { get; }


    /// <summary>
    /// 建造に必要なウェア数量
    /// </summary>
    public long Amount
    {
        get => _amount;
        set
        {
            var oldPrice = Price;
            if (SetProperty(ref _amount, value))
            {
                RaisePropertyChangedEx(oldPrice, Price, nameof(Price));
            }
        }
    }


    /// <summary>
    /// 金額
    /// </summary>
    public long Price => NoBuy ? 0 : Amount * UnitPrice;


    /// <summary>
    /// 単価
    /// </summary>
    public long UnitPrice
    {
        get => _unitPrice;
        set
        {
            // 最低価格≦ 入力価格 ≦ 最高価格かつ価格が変更された場合のみ更新


            var setValue = value;

            if (value < Ware.MinPrice)
            {
                // 入力された値が最低価格未満の場合、最低価格を設定する
                setValue = Ware.MinPrice;
            }
            else if (Ware.MaxPrice < value)
            {
                // 入力された値が最高価格を超える場合、最高価格を設定する
                setValue = Ware.MaxPrice;
            }


            // 変更無しの場合は何もしない
            if (setValue == _unitPrice)
            {
                return;
            }


            var oldUnitPrice = _unitPrice;
            var oldPrice = Price;
            _unitPrice = setValue;

            RaisePropertyChangedEx(oldUnitPrice, setValue);

            if (!NoBuy)
            {
                RaisePropertyChangedEx(oldPrice, Price, nameof(Price));
            }

            EditStatus = EditStatus.Edited;
        }
    }


    /// <summary>
    /// 選択されているか
    /// </summary>
    public bool IsSelected { get; set; }


    /// <summary>
    /// 建造ウェアを購入しない
    /// </summary>
    public bool NoBuy
    {
        get => _noBuy;
        set
        {
            var oldPrice = Price;

            if (SetProperty(ref _noBuy, value))
            {
                RaisePropertyChangedEx(oldPrice, Price, nameof(Price));
                EditStatus = EditStatus.Edited;
            }
        }
    }


    /// <summary>
    /// 編集状態
    /// </summary>
    public EditStatus EditStatus
    {
        get => _editStatus;
        set => SetProperty(ref _editStatus, value);
    }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="wareID">建造に必要なウェアID</param>
    /// <param name="amount">建造に必要なウェア数</param>
    public BuildResourcesGridItem(string wareID, long amount)
    {
        Ware = X4Database.Instance.Ware.Get(wareID);
        UnitPrice = (Ware.MaxPrice + Ware.MinPrice) / 2;
        Amount = amount;
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="wareID">建造に必要なウェアID</param>
    /// <param name="amount">建造に必要なウェア数</param>
    /// <param name="unitPrice">単価</param>
    public BuildResourcesGridItem(string wareID, long amount, long unitPrice)
    {
        Ware = X4Database.Instance.Ware.Get(wareID);
        UnitPrice = unitPrice;
        Amount = amount;
    }




    /// <summary>
    /// 百分率ベースで価格を設定する
    /// </summary>
    /// <param name="percent">百分率の値</param>
    public void SetUnitPricePercent(long percent)
    {
        UnitPrice = (long)(Ware.MinPrice + (Ware.MaxPrice - Ware.MinPrice) * 0.01 * percent);
    }
}

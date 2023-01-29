using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

/// <summary>
/// 労働者数を管理するクラス
/// </summary>
public class WorkforceManager : BindableBaseEx
{
    #region メンバ
    /// <summary>
    /// 現在の労働者数
    /// </summary>
    private long _actual = 0;


    /// <summary>
    /// 最大労働者数
    /// </summary>
    private long _need = 0;


    /// <summary>
    /// 収容人数
    /// </summary>
    private long _capacity = 0;


    /// <summary>
    /// 常に最大にするか
    /// </summary>
    private bool _alwaysMaximum;
    #endregion


    #region プロパティ
    /// <summary>
    /// 現在の労働者数
    /// </summary>
    public long Actual
    {
        get => _actual;
        set
        {
            var oldProportion = Proportion;
            if (SetPropertyEx(ref _actual, value))
            {
                RaisePropertyChangedEx(oldProportion, Proportion, nameof(Proportion));
            }
        }
    }


    /// <summary>
    /// 必要労働者数
    /// </summary>
    public long Need
    {
        get => _need;
        set
        {
            var oldProportion = Proportion;
            if (SetPropertyEx(ref _need, value))
            {
                RaisePropertyChangedEx(oldProportion, Proportion, nameof(Proportion));
            }
        }
    }


    /// <summary>
    /// 収容人数
    /// </summary>
    public long Capacity
    {
        get => _capacity;
        set
        {
            var isActualChange = value < Actual || Actual < value && AlwaysMaximum;
            if (SetPropertyEx(ref _capacity, value) && isActualChange)
            {
                Actual = value;
            }
        }
    }


    /// <summary>
    /// 現在の労働者数と必要労働者数の割合
    /// </summary>
    public double Proportion
    {
        get
        {
            if (Need == 0)
            {
                return 0.0;
            }

            return (double)Actual / Need;
        }
    }


    /// <summary>
    /// 常に最大にするか
    /// </summary>
    public bool AlwaysMaximum
    {
        get => _alwaysMaximum;
        set
        {
            if (SetProperty(ref _alwaysMaximum, value) && value)
            {
                Actual = Capacity;
            }
        }
    }
    #endregion


    /// <summary>
    /// 内容をクリアする
    /// </summary>
    public void Clear()
    {
        Need = 0;
        Capacity = 0;
        Actual = 0;
    }
}

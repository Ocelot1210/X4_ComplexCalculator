using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;

/// <summary>
/// 製品一覧DataGridの＋/－で表示するListViewのアイテム(生産品)
/// </summary>
public class ProductDetailsListItem : BindableBase, IProductDetailsListItem
{
    #region メンバ
    /// <summary>
    /// 製品数(モジュール追加用)
    /// </summary>
    private readonly long _Amount;


    /// <summary>
    /// モジュール数
    /// </summary>
    private long _ModuleCount;


    /// <summary>
    /// 生産性
    /// </summary>
    private readonly Dictionary<string, double> _Efficiencies;


    /// <summary>
    /// 最大生産性
    /// </summary>
    private readonly IReadOnlyDictionary<string, IWareEffect> _MaxEfficiencies;
    #endregion


    #region プロパティ
    /// <inheritdoc/>
    public string WareID { get; }


    //// <inheritdoc/>
    public string ModuleID { get; }


    /// <inheritdoc/>
    public string ModuleName { get; }


    /// <inheritdoc/>
    public long ModuleCount
    {
        get => _ModuleCount;
        set
        {
            if (SetProperty(ref _ModuleCount, value))
            {
                RaisePropertyChanged(nameof(Amount));
            }
        }
    }


    /// <inheritdoc/>
    public long Amount => (long)(Efficiency * _Amount * ModuleCount);


    /// <inheritdoc/>
    public double Efficiency
    {
        get
        {
            var ret = 1.0;

            if (_Efficiencies.ContainsKey("work"))
            {
                ret *= _MaxEfficiencies["work"].Product * _Efficiencies["work"] + 1.0;
            }

            if (_Efficiencies.ContainsKey("sunlight"))
            {
                ret *= _Efficiencies["sunlight"] / 100;
            }

            return ret;
        }
    }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="wareID">親要素のウェアID</param>
    /// <param name="module">モジュールID</param>
    /// <param name="moduleCount">モジュール数</param>
    /// <param name="efficiency">効率</param>
    /// <param name="amount">製品数</param>
    /// <param name="settings">ステーションの設定</param>
    public ProductDetailsListItem(string wareID, IX4Module module, long moduleCount, IReadOnlyDictionary<string, IWareEffect> efficiency, long amount, IStationSettings settings)
    {
        WareID = wareID;
        ModuleID = module.ID;
        ModuleName = module.Name;
        ModuleCount = moduleCount;
        _Amount = amount;
        _MaxEfficiencies = efficiency;

        _Efficiencies = _MaxEfficiencies.ToDictionary(x => x.Key, _ => 1.0);

        if (_Efficiencies.ContainsKey("work"))
        {
            _Efficiencies["work"] = settings.Workforce.Proportion;
        }

        if (_Efficiencies.ContainsKey("sunlight"))
        {
            _Efficiencies["sunlight"] = settings.Sunlight;
        }
    }




    /// <summary>
    /// 生産性を設定
    /// </summary>
    /// <param name="effectID">効果ID</param>
    /// <param name="value">設定値</param>
    public void SetEfficiency(string effectID, double value)
    {
        if (_Efficiencies.ContainsKey(effectID))
        {
            _Efficiencies[effectID] = value;
            RaisePropertyChanged(nameof(Amount));
            RaisePropertyChanged(nameof(Efficiency));
        }
    }
}

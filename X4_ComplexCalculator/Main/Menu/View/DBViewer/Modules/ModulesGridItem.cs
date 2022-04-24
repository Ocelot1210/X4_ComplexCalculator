using Prism.Mvvm;
using System.Linq;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.Menu.View.DBViewer.Modules;

/// <summary>
/// モジュール一覧表示用DataGridの1レコード分
/// </summary>
class ModulesGridItem : BindableBase
{
    #region メンバ
    /// <summary>
    /// 表示対象モジュール
    /// </summary>
    private readonly IX4Module _Module;

    /// <summary>
    /// 製品
    /// </summary>
    private readonly IWare? _Product;
    #endregion


    #region プロパティ
    /// <summary>
    /// モジュール名
    /// </summary>
    public string ModuleName => _Module.Name;


    /// <summary>
    /// モジュール種別
    /// </summary>
    public string ModuleType => _Module.ModuleType.Name;


    /// <summary>
    /// 所有派閥
    /// </summary>
    public string Race { get; }


    /// <summary>
    /// 従業員数
    /// </summary>
    public long MaxWorkers => _Module.MaxWorkers;


    /// <summary>
    /// 収容人数
    /// </summary>
    public long WorkersCapacity => _Module.WorkersCapacity;


    /// <summary>
    /// 製品
    /// </summary>
    public string Product => _Product?.Name ?? "";


    /// <summary>
    /// 最大効率
    /// </summary>
    public long MaxEfficiency { get; }
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="module">表示対象モジュール</param>
    public ModulesGridItem(IX4Module module)
    {
        _Module = module;
        
        // 所有派閥を設定(ある種族固有の場合のみ表示)
        var ownerRaces = _Module.Owners.Select(x => x.Race.Name).OrderBy(x => x).Distinct().ToArray();
        Race = (ownerRaces.Length == 1) ? ownerRaces[0] : "";


        // 製品がある場合、製品の情報を設定
        if (_Module.Products.Any())
        {
            var firstWare = _Module.Products.First();
            _Product = X4Database.Instance.Ware.Get(firstWare.WareID);
            MaxEfficiency = (long)((_Product.WareEffects.TryGet(firstWare.Method, "work")?.Product ?? 0.0 + 1) * 100);
        }
    }
}

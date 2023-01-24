using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;

/// <summary>
/// 建造リソース計算用クラス
/// </summary>
class BuildResourceCalculator
{
    #region スタティックメンバ
    /// <summary>
    /// 建造リソース計算用シングルトンインスタンス
    /// </summary>
    private static BuildResourceCalculator? _SingletonInstance;
    #endregion



    /// <summary>
    /// 建造リソース計算用クラスのインスタンス
    /// </summary>
    public static BuildResourceCalculator Instance
    {
        get
        {
            // 未作成なら作成する
            if (_SingletonInstance is null)
            {
                _SingletonInstance = new BuildResourceCalculator();
            }

            return _SingletonInstance;
        }
    }



    /// <summary>
    /// 建造に必要なウェアを計算
    /// </summary>
    /// <param name="items">建造対象, 建造方法, 建造個数のタプルの列挙</param>
    /// <returns>建造に必要なウェア一覧</returns>
    public IEnumerable<CalcResult> CalcResource(IEnumerable<(IWare Ware, string Method, long Count)> items)
    {
        return items
            .SelectMany(x => CalcResourceInternal(x.Ware, x.Method, x.Count))
            .GroupBy(x => x.WareID)
            .Select(x => new CalcResult(x.Key, x.Sum(y => y.Amount)));
    }


    /// <summary>
    /// 1ウェア単位の建造に必要なウェアを計算
    /// </summary>
    /// <param name="ware">ウェア</param>
    /// <param name="method">建造方法</param>
    /// <param name="count">個数</param>
    /// <returns></returns>
    private IEnumerable<CalcResult> CalcResourceInternal(IWare ware, string method, long count)
    {
        var resources = 
            ware.Resources.TryGetValue(method, out var ret1) ? ret1 :
            ware.Resources.TryGetValue(method, out var ret2) ? ret2 :
            Enumerable.Empty<IWareResource>();

        return resources.Select(x => new CalcResult(x.NeedWareID, x.Amount * count));
    }
}

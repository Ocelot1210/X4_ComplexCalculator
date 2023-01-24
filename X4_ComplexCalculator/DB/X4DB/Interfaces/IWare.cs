using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB.Interfaces;

/// <summary>
/// ウェア情報用インターフェース
/// </summary>
public interface IWare
{
    #region プロパティ
    /// <summary>
    /// ウェアID
    /// </summary>
    public string ID { get; }

    /// <summary>
    /// ウェア名
    /// </summary>
    public string Name { get; }


    /// <summary>
    /// ウェア種別
    /// </summary>
    public IWareGroup WareGroup { get; }


    /// <summary>
    /// カーゴ種別
    /// </summary>
    public ITransportType TransportType { get; }


    /// <summary>
    /// 説明文
    /// </summary>
    public string Description { get; }


    /// <summary>
    /// コンテナサイズ
    /// </summary>
    public long Volume { get; }


    /// <summary>
    /// 最低価格
    /// </summary>
    public long MinPrice { get; }


    /// <summary>
    /// 平均値
    /// </summary>
    public long AvgPrice { get; }


    /// <summary>
    /// 最高価格
    /// </summary>
    public long MaxPrice { get; }


    /// <summary>
    /// 所有派閥
    /// </summary>
    public IReadOnlyList<IFaction> Owners { get; }


    /// <summary>
    /// 生産方式
    /// </summary>
    public IReadOnlyDictionary<string, IWareProduction> Productions { get; }


    /// <summary>
    /// 生産に必要なウェア情報
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<IWareResource>> Resources { get; }


    /// <summary>
    /// タグ一覧
    /// </summary>
    public HashSet<string> Tags { get; }


    /// <summary>
    /// ウェア生産時の追加効果情報
    /// </summary>
    public IWareEffects WareEffects { get; }
    #endregion


    #region メソッド
    /// <summary>
    /// 他のウェアと比較し、同一であるか判定する
    /// </summary>
    /// <param name="other">比較先</param>
    /// <returns>同一であるか</returns>
    public bool Equals(IWare other) => ID == other.ID;


    /// <summary>
    /// 生産方式に対応する生産情報を取得する
    /// </summary>
    /// <param name="method">生産方式</param>
    /// <returns>生産情報</returns>
    public IWareProduction? TryGetProduction(string method)
    {
        // 生産方式に対応する生産情報を取得
        if (Productions.TryGetValue(method, out var production))
        {
            return production;
        }

        // デフォルトの生産情報を取得
        if (Productions.TryGetValue("default", out var defaultProduction))
        {
            return defaultProduction;
        }

        // 取得失敗
        return null;
    }
    #endregion
}

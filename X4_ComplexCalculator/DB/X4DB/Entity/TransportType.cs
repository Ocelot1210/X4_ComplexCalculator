using System;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// カーゴタイプ(輸送種別)情報用クラス
/// </summary>
public class TransportType : ITransportType
{
    #region ITransportType
    /// <inheritdoc/>
    public string TransportTypeID { get; }


    /// <inheritdoc/>
    public string Name { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="transportTypeID">カーゴ種別ID</param>
    /// <param name="name">カーゴ種別名</param>
    public TransportType(string transportTypeID, string name)
    {
        TransportTypeID = transportTypeID;
        Name = name;
    }


    /// <summary>
    /// 比較
    /// </summary>
    /// <param name="obj">比較対象</param>
    /// <returns></returns>
    public override bool Equals(object? obj) => obj is ITransportType other && Equals(other);


    /// <summary>
    /// 比較
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(ITransportType other) => TransportTypeID == other.TransportTypeID;


    /// <summary>
    /// ハッシュ値を取得
    /// </summary>
    /// <returns>ハッシュ値</returns>
    public override int GetHashCode() => HashCode.Combine(TransportTypeID);
}

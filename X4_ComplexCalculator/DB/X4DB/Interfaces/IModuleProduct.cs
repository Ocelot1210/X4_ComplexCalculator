﻿namespace X4_ComplexCalculator.DB.X4DB.Interfaces;

/// <summary>
/// モジュールの製品情報用インターフェイス
/// </summary>
public interface IModuleProduct
{
    #region プロパティ
    /// <summary>
    /// モジュールID
    /// </summary>
    public string ModuleID { get; }


    /// <summary>
    /// 生産対象のウェアID
    /// </summary>
    public string WareID { get; }


    /// <summary>
    /// 製造方式
    /// </summary>
    public string Method { get; }


    /// <summary>
    /// 生産量
    /// </summary>
    public long Amount { get; }


    /// <summary>
    /// 生産情報
    /// </summary>
    public IWareProduction WareProduction { get; }
    #endregion
}

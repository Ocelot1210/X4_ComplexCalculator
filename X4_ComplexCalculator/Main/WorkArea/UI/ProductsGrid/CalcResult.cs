using System.Collections.Generic;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;

/// <summary>
/// 製品の計算結果を表すrecord
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="WareID">生産/消費ウェアID</param>
/// <param name="WareAmount">ウェア生産/消費量</param>
/// <param name="Method">ウェア生産方式</param>
/// <param name="Module">生産/消費ウェアを生産/消費するモジュール</param>
/// <param name="ModuleCount">生産/消費ウェアを生産/消費するモジュールの数</param>
/// <param name="Efficiency">ウェア生産の追加効果</param>
sealed record CalcResult(string WareID, long WareAmount, string Method, IX4Module Module, long ModuleCount, IReadOnlyDictionary<string, IWareEffect>? Efficiency = null)
{

}

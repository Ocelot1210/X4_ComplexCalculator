using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// モジュールの製品情報用クラス
    /// </summary>
    /// <param name="ModuleID">モジュールID</param>
    /// <param name="WareID">生産対象のウェアID</param>
    /// <param name="Method">製造方式</param>
    /// <param name="WareProduction">生産情報</param>
    public sealed record ModuleProduct(
        string ModuleID,
        string WareID,
        string Method,
        IWareProduction WareProduction
    ) : IModuleProduct;
}

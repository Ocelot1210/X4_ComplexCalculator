using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// 1サイクルのウェア生産に必要なウェア情報用クラス
    /// </summary>
    /// <param name="WareID">ウェアID</param>
    /// <param name="Method">生産方式</param>
    /// <param name="NeedWareID">必要ウェアID</param>
    /// <param name="Amount">必要量</param>
    public sealed record WareResource(
        string WareID,
        string Method,
        string NeedWareID,
        long Amount
    ) : IWareResource;
}

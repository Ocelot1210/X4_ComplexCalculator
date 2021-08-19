using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// ウェアの生産量と生産時間情報用クラス
    /// </summary>
    /// <param name="WareID">ウェアID</param>
    /// <param name="Method">生産方式</param>
    /// <param name="Name">名称</param>
    /// <param name="Amount">生産量</param>
    /// <param name="Time">生産時間</param>
    public sealed record WareProduction(
        string WareID,
        string Method,
        string Name,
        long Amount,
        double Time
    ) : IWareProduction
    {
        /// <inheritdoc/>
        public override string ToString() => Name;
    }
}

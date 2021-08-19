namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// エンジンの推進力情報用クラス
    /// </summary>
    /// <param name="Forward">前方推進力</param>
    /// <param name="Reverse">後方推進力</param>
    /// <param name="Boost">ブースト推進力</param>
    /// <param name="Travel">トラベル推進力</param>
    public sealed record EngineThrust(
        double Forward,
        double Reverse,
        double Boost,
        double Travel
    );
}

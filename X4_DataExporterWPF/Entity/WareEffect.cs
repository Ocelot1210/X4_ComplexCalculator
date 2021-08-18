namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア生産時の追加効果
    /// </summary>
    /// <param name="WareID">ウェアID</param>
    /// <param name="Method">ウェア生産方式</param>
    /// <param name="EffectID">追加効果ID</param>
    /// <param name="Product">生産倍率</param>
    public sealed record WareEffect(string WareID, string Method, string EffectID, double Product);
}

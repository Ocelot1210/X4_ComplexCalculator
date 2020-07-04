namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア生産時の追加効果
    /// </summary>
    public class WareEffect
    {
        public string WareID { get; }

        public string Method { get; }

        public string EffectID { get; }

        public double Product { get; }

        public WareEffect(string wareID, string method, string effectID, double product)
        {
            this.WareID = wareID;
            this.Method = method;
            this.EffectID = effectID;
            this.Product = product;
        }
    }
}

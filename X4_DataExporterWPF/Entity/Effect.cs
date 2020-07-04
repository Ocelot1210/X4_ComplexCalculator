namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア生産時の追加効果
    /// </summary>
    public class Effect
    {
        public string EffectID { get; }

        public string Name { get; }

        public Effect(string effectID, string name)
        {
            this.EffectID = effectID;
            this.Name = name;
        }
    }
}

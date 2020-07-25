namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア生産時の追加効果
    /// </summary>
    public class Effect
    {
        #region プロパティ
        /// <summary>
        /// 効果ID
        /// </summary>
        public string EffectID { get; }


        /// <summary>
        /// 効果名
        /// </summary>
        public string Name { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="effectID">効果ID</param>
        /// <param name="name">効果名</param>
        public Effect(string effectID, string name)
        {
            EffectID = effectID;
            Name = name;
        }
    }
}

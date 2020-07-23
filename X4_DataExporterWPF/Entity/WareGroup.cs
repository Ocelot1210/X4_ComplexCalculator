namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア種別
    /// </summary>
    public class WareGroup
    {
        #region プロパティ
        /// <summary>
        /// ウェア種別ID
        /// </summary>
        public string WareGroupID { get; }


        /// <summary>
        /// ウェア種別名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 工場名
        /// </summary>
        public string FactoryName { get; }


        /// <summary>
        /// アイコン名
        /// </summary>
        public string Icon { get; }


        /// <summary>
        /// 階級
        /// </summary>
        public int Tier { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareGroupID">ウェア種別ID</param>
        /// <param name="name">ウェア種別名</param>
        /// <param name="factoryName">工場名</param>
        /// <param name="icon">アイコン名</param>
        /// <param name="tier">階級</param>
        public WareGroup(string wareGroupID, string name, string factoryName, string icon, int tier)
        {
            WareGroupID = wareGroupID;
            Name = name;
            FactoryName = factoryName;
            Icon = icon;
            Tier = tier;
        }
    }
}

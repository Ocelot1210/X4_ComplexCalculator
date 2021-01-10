namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 派閥
    /// </summary>
    public class Faction
    {
        #region プロパティ
        /// <summary>
        /// 派閥ID
        /// </summary>
        public string FactionID { get; }


        /// <summary>
        /// 派閥名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 種族ID
        /// </summary>
        public string RaceID { get; }


        /// <summary>
        /// 派閥略称
        /// </summary>
        public string ShortName { get; }


        /// <summary>
        /// 説明文
        /// </summary>
        public string Description { get; }


        /// <summary>
        /// アイコン画像
        /// </summary>
        public byte[]? Icon { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="factionID">派閥ID</param>
        /// <param name="name">派閥名</param>
        /// <param name="raceID">種族ID</param>
        /// <param name="shortName">派閥略称</param>
        /// <param name="description">説明文</param>
        /// <param name="icon">アイコン画像</param>
        public Faction(string factionID, string name, string raceID, string shortName, string description, byte[]? icon)
        {
            FactionID = factionID;
            Name = name;
            RaceID = raceID;
            ShortName = shortName;
            Description = description;
            Icon = icon;
        }
    }
}

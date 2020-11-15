namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 種族
    /// </summary>
    public class Race
    {
        #region プロパティ
        /// <summary>
        /// 種族ID
        /// </summary>
        public string RaceID { get; }


        /// <summary>
        /// 種族名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 種族略称
        /// </summary>
        public string ShortName { get; }


        /// <summary>
        /// 説明文
        /// </summary>
        public string Description { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="raceID">種族ID</param>
        /// <param name="name">種族名</param>
        /// <param name="shortName">種族略称</param>
        public Race(string raceID, string name, string shortName, string description)
        {
            RaceID = raceID;
            Name = name;
            ShortName = shortName;
            Description = description;
        }
    }
}

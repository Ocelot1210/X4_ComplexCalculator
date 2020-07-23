namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュール所有派閥
    /// </summary>
    public class ModuleOwner
    {
        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ModuleID { get; }


        /// <summary>
        /// 派閥ID
        /// </summary>
        public string FactionID { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="factionID">派閥ID</param>
        public ModuleOwner(string moduleID, string factionID)
        {
            ModuleID = moduleID;
            FactionID = factionID;
        }
    }
}

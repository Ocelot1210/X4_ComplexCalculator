namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュールの保管庫種別
    /// </summary>
    public class ModuleStorageType
    {
        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ModuleID { get; }


        /// <summary>
        /// 保管庫種別
        /// </summary>
        public string TransportTypeID { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="transportTypeID">保管庫種別</param>
        public ModuleStorageType(string moduleID, string transportTypeID)
        {
            ModuleID = moduleID;
            TransportTypeID = transportTypeID;
        }
    }
}

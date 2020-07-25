namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュールの保管容量
    /// </summary>
    public class ModuleStorage
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


        /// <summary>
        /// 保管庫容量
        /// </summary>
        public int Amount { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="transportTypeID">保管庫種別</param>
        /// <param name="amount">保管庫容量</param>
        public ModuleStorage(string moduleID, string transportTypeID, int amount)
        {
            ModuleID = moduleID;
            TransportTypeID = transportTypeID;
            Amount = amount;
        }
    }
}

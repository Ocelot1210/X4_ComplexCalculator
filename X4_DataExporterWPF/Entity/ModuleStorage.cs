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
        /// 保管庫容量
        /// </summary>
        public int Amount { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="amount">保管庫容量</param>
        public ModuleStorage(string moduleID, int amount)
        {
            ModuleID = moduleID;
            Amount = amount;
        }
    }
}

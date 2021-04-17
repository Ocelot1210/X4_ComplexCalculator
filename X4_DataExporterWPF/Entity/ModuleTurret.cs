namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュールのタレット
    /// </summary>
    public class ModuleTurret
    {
        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ModuleID { get; }


        /// <summary>
        /// サイズID
        /// </summary>
        public string SizeID { get; }


        /// <summary>
        /// 装備可能個数
        /// </summary>
        public long Amount { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="sizeID">サイズID</param>
        /// <param name="amount">装備可能個数</param>
        public ModuleTurret(string moduleID, string sizeID, long amount)
        {
            ModuleID = moduleID;
            SizeID = sizeID;
            Amount = amount;
        }
    }
}

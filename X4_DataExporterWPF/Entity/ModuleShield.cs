namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュールのシールド
    /// </summary>
    public class ModuleShield
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
        public int Amount { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="sizeID">サイズID</param>
        /// <param name="amount">装備可能個数</param>
        public ModuleShield(string moduleID, string sizeID, int amount)
        {
            ModuleID = moduleID;
            SizeID = sizeID;
            Amount = amount;
        }
    }
}

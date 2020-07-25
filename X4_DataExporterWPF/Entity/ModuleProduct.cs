namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュールの生産品
    /// </summary>
    public class ModuleProduct
    {
        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ModuleID { get; }


        /// <summary>
        /// 生産ウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// 生産方式
        /// </summary>
        public string Method { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="wareID">生産ウェアID</param>
        /// <param name="method">生産方式</param>
        public ModuleProduct(string moduleID, string wareID, string method)
        {
            ModuleID = moduleID;
            WareID = wareID;
            Method = method;
        }
    }
}

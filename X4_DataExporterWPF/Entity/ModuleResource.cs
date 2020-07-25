namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュール建造に必要なウェア情報
    /// </summary>
    public class ModuleResource
    {
        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ModuleID { get; }


        /// <summary>
        /// 建造方式
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// 建造に必要なウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// 建造に必要なウェア数
        /// </summary>
        public int Amount { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="method">建造方式</param>
        /// <param name="wareID">建造に必要なウェアID</param>
        /// <param name="amount">建造に必要なウェア数</param>
        public ModuleResource(string moduleID, string method, string wareID, int amount)
        {
            ModuleID = moduleID;
            Method = method;
            WareID = wareID;
            Amount = amount;
        }
    }
}

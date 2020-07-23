namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュール建造に関する情報
    /// </summary>
    public class ModuleProduction
    {
        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ModuleID { get; }


        /// <summary>
        /// モジュール建造方式
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// 建造時間
        /// </summary>
        public double Time { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="method">モジュール建造方式</param>
        /// <param name="time">建造時間</param>
        public ModuleProduction(string moduleID, string method, double time)
        {
            ModuleID = moduleID;
            Method = method;
            Time = time;
        }
    }
}

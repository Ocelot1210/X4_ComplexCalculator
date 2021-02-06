namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュールの製品情報管理クラス
    /// </summary>
    public class ModuleProduct
    {
        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ModuleID { get; }


        /// <summary>
        /// 生産対象のウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// 製造方式
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// 生産情報
        /// </summary>
        public WareProduction WareProduction { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="wareID">ウェアID</param>
        /// <param name="method">製造方式</param>
        /// <param name="production">生産情報</param>
        public ModuleProduct(string moduleID, string wareID, string method, WareProduction production)
        {
            ModuleID = moduleID;
            WareID = wareID;
            Method = method;
            WareProduction = production;
        }
    }
}

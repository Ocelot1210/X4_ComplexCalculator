using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// モジュールの製品情報用クラス
    /// </summary>
    public class ModuleProduct : IModuleProduct
    {
        #region IModuleProduct
        /// <inheritdoc/>
        public string ModuleID { get; }


        /// <inheritdoc/>
        public string WareID { get; }


        /// <inheritdoc/>
        public string Method { get; }


        /// <inheritdoc/>
        public IWareProduction WareProduction { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="wareID">ウェアID</param>
        /// <param name="method">製造方式</param>
        /// <param name="production">生産情報</param>
        public ModuleProduct(string moduleID, string wareID, string method, IWareProduction production)
        {
            ModuleID = moduleID;
            WareID = wareID;
            Method = method;
            WareProduction = production;
        }
    }
}

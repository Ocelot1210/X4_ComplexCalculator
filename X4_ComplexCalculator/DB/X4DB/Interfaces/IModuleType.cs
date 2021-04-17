namespace X4_ComplexCalculator.DB.X4DB.Interfaces
{
    /// <summary>
    /// モジュール種別情報用インターフェイス
    /// </summary>
    public interface IModuleType
    {
        #region プロパティ
        /// <summary>
        /// モジュール種別ID
        /// </summary>
        public string ModuleTypeID { get; }


        /// <summary>
        /// モジュール種別名
        /// </summary>
        public string Name { get; }
        #endregion
    }
}
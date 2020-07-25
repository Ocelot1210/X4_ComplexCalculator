namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュール種別
    /// </summary>
    public class ModuleType
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


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleTypeID">モジュール種別ID</param>
        /// <param name="name">モジュール種別名</param>
        public ModuleType(string moduleTypeID, string name)
        {
            ModuleTypeID = moduleTypeID;
            Name = name;
        }
    }
}

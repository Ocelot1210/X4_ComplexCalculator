namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュール
    /// </summary>
    public class Module
    {
        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ModuleID { get; }


        /// <summary>
        /// モジュール種別ID
        /// </summary>
        public string ModuleTypeID { get; }


        /// <summary>
        /// モジュール名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// マクロ名
        /// </summary>
        public string Macro { get; }


        /// <summary>
        /// 最大労働者数
        /// </summary>
        public int MaxWorkers { get; }


        /// <summary>
        /// 収容可能な労働者数
        /// </summary>
        public int WorkersCapacity { get; }


        /// <summary>
        /// 設計図有無
        /// </summary>
        public int NoBlueprint { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="moduleTypeID">ModuleTypeID</param>
        /// <param name="name">Name</param>
        /// <param name="macro">マクロ名</param>
        /// <param name="maxWorkers">最大労働者数</param>
        /// <param name="workersCapacity">WorkersCapacity</param>
        /// <param name="noBluePrint">設計図有無</param>
        public Module(
            string moduleID, string moduleTypeID, string name,
            string macro, int maxWorkers, int workersCapacity,
            int noBluePrint
        )
        {
            ModuleID = moduleID;
            ModuleTypeID = moduleTypeID;
            Name = name;
            Macro = macro;
            MaxWorkers = maxWorkers;
            WorkersCapacity = workersCapacity;
            NoBlueprint = noBluePrint;
        }

    }
}

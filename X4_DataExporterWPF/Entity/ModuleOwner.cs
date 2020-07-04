namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュール所有派閥情報抽出用クラス
    /// </summary>
    public class ModuleOwner
    {
        public string ModuleID { get; }

        public string FactionID { get; }

        public ModuleOwner(string moduleID, string factionID)
        {
            this.ModuleID = moduleID;
            this.FactionID = factionID;
        }
    }
}

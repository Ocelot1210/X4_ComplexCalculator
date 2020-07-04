namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア種別
    /// </summary>
    public class WareGroup
    {
        public string WareGroupID { get; }

        public string Name { get; }

        public string FactoryName { get; }

        public string Icon { get; }

        public int Tier { get; }

        public WareGroup(string wareGroupID, string name, string factoryName, string icon, int tier)
        {
            this.WareGroupID = wareGroupID;
            this.Name = name;
            this.FactoryName = factoryName;
            this.Icon = icon;
            this.Tier = tier;
        }
    }
}

namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア
    /// </summary>
    public class Ware
    {
        public string WareID { get; }

        public string WareGroupID { get; }

        public string TransportTypeID { get; }

        public string Name { get; }

        public string Description { get; }

        public string FactoryName { get; }

        public int Volume { get; }

        public int MinPrice { get; }

        public int AvgPrice { get; }

        public int MaxPrice { get; }

        public Ware(
            string wareID, string wareGroupID, string transportTypeID,
            string name, string description, string factoryName,
            int volume, int minPrice, int avgPrice, int maxPrice
        )
        {
            this.WareID = wareID;
            this.WareGroupID = wareGroupID;
            this.TransportTypeID = transportTypeID;
            this.Name = name;
            this.Description = description;
            this.FactoryName = factoryName;
            this.Volume = volume;
            this.MinPrice = minPrice;
            this.AvgPrice = avgPrice;
            this.MaxPrice = maxPrice;
        }
    }
}

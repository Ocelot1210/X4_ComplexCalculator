namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// カーゴ種別
    /// </summary>
    public class TransportType
    {
        public string TransportTypeID { get; }

        public string Name { get; }

        public TransportType(string transportTypeID, string name)
        {
            this.TransportTypeID = transportTypeID;
            this.Name = name;
        }
    }
}

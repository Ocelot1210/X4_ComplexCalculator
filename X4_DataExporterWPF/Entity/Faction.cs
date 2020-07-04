namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 派閥
    /// </summary>
    public class Faction
    {
        public string FactionID { get; }

        public string Name { get; }

        public string RaceID { get; }

        public string ShortName { get; }

        public Faction(string factionID, string name, string raceID, string shortName)
        {
            this.FactionID = factionID;
            this.Name = name;
            this.RaceID = raceID;
            this.ShortName = shortName;
        }
    }
}

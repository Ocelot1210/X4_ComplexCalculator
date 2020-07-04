namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 種族
    /// </summary>
    public class Race
    {
        public string RaceID { get; }

        public string Name { get; }

        public string ShortName { get; }

        public Race(string raceID, string name, string shortName)
        {
            this.RaceID = raceID;
            this.Name = name;
            this.ShortName = shortName;
        }
    }
}

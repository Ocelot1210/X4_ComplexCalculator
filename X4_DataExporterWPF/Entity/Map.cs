namespace X4_DataExporterWPF.Entity
{
    public class Map
    {
        public string Macro { get; }

        public string Name { get; }

        public string Description { get; }

        public Map(string macro, string name, string description)
        {
            this.Macro = macro;
            this.Name = name;
            this.Description = description;
        }
    }
}

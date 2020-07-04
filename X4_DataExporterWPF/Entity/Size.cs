namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// サイズ
    /// </summary>
    public class Size
    {
        public string SizeID { get; }

        public string Name { get; }

        public Size(string sizeID, string name)
        {
            this.SizeID = sizeID;
            this.Name = name;
        }
    }
}

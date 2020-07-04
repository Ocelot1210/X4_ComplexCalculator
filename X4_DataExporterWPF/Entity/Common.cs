namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// データベースメタデータ
    /// </summary>
    public class Common
    {
        public string Item { get; }

        public int Value { get; }

        public Common(string item, int value)
        {
            this.Item = item;
            this.Value = value;
        }
    }
}

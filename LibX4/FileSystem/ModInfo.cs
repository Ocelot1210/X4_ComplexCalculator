using System.IO;
using System.Xml.Linq;

namespace X4_DataExporterWPF.Export
{

    /// <summary>
    /// Modの情報
    /// </summary>
    class ModInfo
    {
        /// <summary>
        /// 識別ID
        /// </summary>
        public string ID { get; }


        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; }


        /// <summary>
        /// バージョン
        /// </summary>
        public string Version { get; }


        /// <summary>
        /// 作成日時
        /// </summary>
        public string Date { get; }


        /// <summary>
        /// 有効化されているか
        /// </summary>
        public string Enabled { get; }


        /// <summary>
        /// 保存ファイルに影響を与えるか？
        /// </summary>
        public string Save { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modDirPath">Modのフォルダパス(絶対パスで指定すること)</param>
        public ModInfo(string modDirPath)
        {
            // xml version="1.1"のModがまれに存在するため、XML宣言を読み飛ばす
            using var sr = new StreamReader(Path.Combine(modDirPath, "content.xml"));
            sr.ReadLine();

            var xml = XDocument.Load(sr);

            ID      = xml.Root.Attribute("id")?.Value      ?? "";
            Name    = xml.Root.Attribute("name")?.Value    ?? "";
            Author  = xml.Root.Attribute("author")?.Value  ?? "";
            Version = xml.Root.Attribute("version")?.Value ?? "";
            Date    = xml.Root.Attribute("date")?.Value    ?? "";
            Enabled = xml.Root.Attribute("enabled")?.Value ?? "";
            Save    = xml.Root.Attribute("save")?.Value    ?? "";
        }
    }
}

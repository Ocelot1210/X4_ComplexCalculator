using System.IO;
using System.Xml.Linq;

namespace LibX4.FileSystem
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
            using var fs = new FileStream(Path.Combine(modDirPath, "content.xml"), FileMode.Open, FileAccess.Read);

            // xml宣言を読み飛ばす
            SkipXmlDefined(fs);

            var xml = XDocument.Load(fs);

            ID      = xml.Root.Attribute("id")?.Value      ?? "";
            Name    = xml.Root.Attribute("name")?.Value    ?? "";
            Author  = xml.Root.Attribute("author")?.Value  ?? "";
            Version = xml.Root.Attribute("version")?.Value ?? "";
            Date    = xml.Root.Attribute("date")?.Value    ?? "";
            Enabled = xml.Root.Attribute("enabled")?.Value ?? "";
            Save    = xml.Root.Attribute("save")?.Value    ?? "";
        }


        /// <summary>
        /// xml宣言を読み飛ばす
        /// </summary>
        /// <param name="fs">読み飛ばす対象</param>
        /// <remarks>
        /// xml version="1.1"のModがまれに存在するため、XML宣言を読み飛ばす
        /// </remarks>
        private static void SkipXmlDefined(FileStream fs)
        {
            var sr = new StreamReader(fs);

            // xml宣言が記載されているか判定し、記載されていなければ何もしないで抜ける
            {
                char[] buff = new char[5];

                sr.Read(buff, 0, buff.Length);

                if (new string(buff) != "<?xml")
                {
                    fs.Position = 0;
                    return;
                }
                fs.Position = buff.Length;
            }

            var isDblQt = false;        // ダブルクォート内か
            var isSngQt = false;        // シングルクォート内か
            var endOfXmlDef = false;    // xml宣言を読み飛ばし終えたか
            var seek = 0;               // シーク量

            // xml宣言を読み飛ばす
            do
            {
                var chr = sr.Read();
                seek++;
                if (chr < 0)
                {
                    break;
                }

                switch ((char)chr)
                {
                    case '\'':
                        // ダブルクォートで囲われていないシングルクォートか？
                        if (!isDblQt)
                        {
                            isSngQt = !isSngQt;
                        }
                        break;

                    case '\"':
                        // シングルクォートで囲われていないダブルクォートか？
                        if (!isSngQt)
                        {
                            isDblQt = !isDblQt;
                        }
                        break;

                    case '?':
                        // ダブルクォートまたはシングルクォートに囲われていない'?'か？
                        if (!isSngQt && !isDblQt)
                        {
                            chr = sr.Read();
                            seek++;

                            // xml宣言の終了か？
                            if (chr < 0 || (char)chr == '>')
                            {
                                endOfXmlDef = true;
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
            while (!sr.EndOfStream && !endOfXmlDef);

            fs.Position += seek;
        }
    }
}

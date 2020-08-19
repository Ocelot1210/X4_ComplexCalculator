using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace LibX4.Xml
{
    /// <summary>
    /// XML v1.1 を扱える XDocument.Load を提供するクラス
    /// </summary>
    internal static class XDocumentEx
    {
        /// <summary>
        /// 絶対パスから XDocument を生成する
        /// </summary>
        /// <param name="url">ファイル名</param>
        /// <returns>生成した XDocument</returns>
        public static XDocument Load(string url)
        {
            using var stream = new FileStream(url, FileMode.Open, FileAccess.Read);
            return Load(stream, url);
        }


        /// <summary>
        /// ストリームから XDocument を生成する
        /// </summary>
        /// <param name="stream">XML データのストリーム</param>
        /// <param name="baseUri">XML のベース URL</param>
        /// <returns>生成した XDocument</returns>
        public static XDocument Load(Stream stream, string? baseUri = null)
        {
            // xml宣言の文字数カウント
            var xmlDefCount = CountXmlDefined(stream);
            stream.Position = 0;

            // xml宣言を読み飛ばす
            using var streamReader = new StreamReader(stream);
            for (var cnt = 0; cnt < xmlDefCount; cnt++)
            {
                streamReader.Read();
            }

            using var xmlReader = XmlReader.Create(streamReader, null, baseUri);
            return XDocument.Load(xmlReader);
        }


        /// <summary>
        /// xml宣言の文字数をカウントする
        /// </summary>
        /// <param name="stream">カウント対象</param>
        /// <returns>xml宣言の文字数</returns>
        private static int CountXmlDefined(Stream stream)
        {
            var ret = 0;
            var reader = new StreamReader(stream);

            // xml宣言が記載されているか判定し、記載されていなければ0を返す
            {
                char[] buff = new char[5];

                reader.Read(buff, 0, buff.Length);

                if (new string(buff) != "<?xml")
                {
                    return 0;
                }

                ret += buff.Length;
            }


            var isDblQt = false;        // ダブルクォート内か
            var isSngQt = false;        // シングルクォート内か
            var endOfXmlDef = false;    // xml宣言を読み飛ばし終えたか

            // xml宣言を読み飛ばす
            do
            {
                var chr = reader.Read();
                ret++;
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
                            chr = reader.Read();
                            ret++;

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
            while (!reader.EndOfStream && !endOfXmlDef);

            return ret;
        }
    }
}

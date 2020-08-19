using System;
using System.IO;
using System.Text;
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
        /// XML 宣言の文頭を UTF-8 で表した配列
        /// </summary>
        private static readonly byte[] XmlDeclaration = Encoding.UTF8.GetBytes("<?xml");


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
            stream = SkipXmlDeclaration(stream);
            var xmlReader = XmlReader.Create(stream, null, baseUri);
            return XDocument.Load(xmlReader);
        }


        /// <summary>
        /// ストリームの XML 宣言を読み飛ばす
        /// </summary>
        /// <param name="stream">XML のストリーム</param>
        /// <returns>XML 宣言を読み飛ばしたストリーム</returns>
        private static Stream SkipXmlDeclaration(Stream stream)
        {
            Span<byte> buff = stackalloc byte[58]; // XML 宣言の全属性を指定した場合の文字数
            stream.Read(buff);

            // UTF-8 の BOM を読み飛ばす
            int seek = buff.StartsWith(Encoding.UTF8.Preamble) ? Encoding.UTF8.Preamble.Length : 0;

            // XML 宣言が省略されている場合はそのまま返す
            if (!buff.Slice(seek).StartsWith(XmlDeclaration))
            {
                stream.Position = seek;
                return stream;
            }

            // XML 宣言部分を読み飛ばす
            for (seek += 6; seek < buff.Length; seek++)
            {
                switch (buff[seek])
                {
                    case (byte)'v':
                        seek += 12; // skip 'version="1.x"'
                        break;

                    case (byte)'e':
                        seek += 13; // skip 'encoding="UTF-8"'
                        break;

                    case (byte)'s':
                        seek += 14; // skip 'standalone="no"'
                        break;

                    case (byte)'?':
                        seek += 2;  // skip '?>'
                        stream.Position = seek;
                        return stream;
                }
            }
            throw new InvalidDataException("XML declaration has unexpected length."
                + Environment.NewLine + $"Buff: {Encoding.UTF8.GetString(buff)}");
        }
    }
}

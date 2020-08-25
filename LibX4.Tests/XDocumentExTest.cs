using System.IO;
using System.Linq;
using System.Text;
using LibX4.Xml;
using Xunit;

namespace LibX4.Tests
{
    /// <summary>
    /// <see cref="XDocumentEx"/> のテストクラス
    /// </summary>
    public class XDocumentExTest
    {
        /// <summary>
        /// UTF8 の BOM
        /// </summary>
        private static readonly byte[] BOM = Encoding.UTF8.GetPreamble();


        /// <summary>
        /// UTF8 エンコードした文字列をストリームに変換する
        /// </summary>
        /// <param name="source">文字列</param>
        /// <returns>UTF8 ストリーム</returns>
        private Stream Utf8(string source) => new MemoryStream(Encoding.UTF8.GetBytes(source));


        /// <summary>
        /// UTF8 (BOM 付き) エンコードした文字列をストリームに変換する
        /// </summary>
        /// <param name="source">文字列</param>
        /// <returns>UTF8 ストリーム</returns>
        private Stream Utf8Bom(string source)
            => new MemoryStream(BOM.Concat(Encoding.UTF8.GetBytes(source)).ToArray());


        /// <summary>
        /// XML v1.1 が読み込めることを確認する
        /// </summary>
        [Fact]
        public void Xml11CanBeRead()
        {
            using var s0 = Utf8("<root></root>");
            XDocumentEx.Load(s0);

            using var s1 = Utf8(@"<?xml version=""1.1""?><root></root>");
            XDocumentEx.Load(s1);

            using var s2 = Utf8(@"<?xml version=""1.1"" encoding=""UTF-8""?><root></root>");
            XDocumentEx.Load(s2);

            using var s3 = Utf8(
                @"<?xml version=""1.1"" encoding=""UTF-8"" standalone=""no""?><root></root>");
            XDocumentEx.Load(s3);

            using var s4 = Utf8(
                @"<?xml version=""1.1"" encoding=""UTF-8"" standalone=""yes""?><root></root>");
            XDocumentEx.Load(s4);
        }


        /// <summary>
        /// BOM 付き UTF8 が読み込めることを確認する
        /// </summary>
        [Fact]
        public void Utf8BomCanBeRead()
        {
            using var s0 = Utf8Bom("<root></root>");
            XDocumentEx.Load(s0);

            using var s1 = Utf8Bom(@"<?xml version=""1.1""?><root></root>");
            XDocumentEx.Load(s1);

            using var s2 = Utf8Bom(@"<?xml version=""1.1"" encoding=""UTF-8""?><root></root>");
            XDocumentEx.Load(s2);

            using var s3 = Utf8Bom(
                @"<?xml version=""1.1"" encoding=""UTF-8"" standalone=""no""?><root></root>");
            XDocumentEx.Load(s3);

            using var s4 = Utf8Bom(
                @"<?xml version=""1.1"" encoding=""UTF-8"" standalone=""yes""?><root></root>");
            XDocumentEx.Load(s4);
        }
    }
}

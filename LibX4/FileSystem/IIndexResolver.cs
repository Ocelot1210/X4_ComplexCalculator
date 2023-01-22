using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LibX4.FileSystem
{
    /// <summary>
    /// マクロなどの外部 XML 参照を解決するインターフェース
    /// </summary>
    public interface IIndexResolver
    {
        /// <summary>
        /// Index ファイルに記載されているxmlを開く
        /// </summary>
        /// <param name="indexFilePath">Index ファイルパス</param>
        /// <param name="name">マクロ名等</param>
        /// <param name="cancellationToken">キャンセル トークン</param>
        /// <exception cref="FileNotFoundException">インデックスファイルに該当する名前が記載されていない場合</exception>
        /// <returns>解決結果先のファイル</returns>
        public Task<XDocument> OpenIndexXmlAsync(string indexFilePath, string name, CancellationToken cancellationToken = default);
    }
}

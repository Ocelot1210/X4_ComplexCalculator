using System.IO;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibX4.FileSystem
{
    /// <summary>
    /// X4 の Cat ファイルを操作するクラスのI/F
    /// </summary>
    public interface ICatFile : IIndexResolver
    {
        /// <summary>
        /// 指定したファイルを開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="cancellationToken">キャンセル トークン</param>
        /// <exception cref="FileNotFoundException">該当ファイルが無い</exception>
        /// <returns>ファイルの内容</returns>
        public Task<Stream> OpenFileAsync(string filePath, CancellationToken cancellationToken = default);


        /// <summary>
        /// 指定したファイルのオープンをを試みる
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="cancellationToken">キャンセル トークン</param>
        /// <returns>ファイルの内容又はnull</returns>
        public Task<Stream?> TryOpenFileAsync(string filePath, CancellationToken cancellationToken = default);


        /// <summary>
        /// XML ファイルを開く
        /// </summary>
        /// <param name="filePath">開くファイルの相対パス</param>
        /// <param name="cancellationToken">キャンセル トークン</param>
        /// <exception cref="FileNotFoundException">該当ファイルが無い</exception>
        /// <returns>開いた XML 文書</returns>
        public Task<XDocument> OpenXmlAsync(string filePath, CancellationToken cancellationToken = default);


        /// <summary>
        /// XML ファイルの読み込みを試みる
        /// </summary>
        /// <param name="filePath">開くファイルの相対パス</param>
        /// <param name="cancellationToken">キャンセル トークン</param>
        /// <returns>開いた XML 文書、該当ファイルが無かった場合は null</returns>
        public Task<XDocument?> TryOpenXmlAsync(string filePath, CancellationToken cancellationToken = default);
    }
}

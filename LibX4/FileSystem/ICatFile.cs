using System.IO;
using System.Xml.Linq;

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
        /// <exception cref="FileNotFoundException">該当ファイルが無い</exception>
        /// <returns>ファイルの内容</returns>
        public Stream OpenFile(string filePath);


        /// <summary>
        /// 指定したファイルのオープンをを試みる
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>ファイルの内容又はnull</returns>
        public Stream? TryOpenFile(string filePath);


        /// <summary>
        /// XML ファイルの読み込みを試みる
        /// </summary>
        /// <param name="filePath">開くファイルの相対パス</param>
        /// <returns>開いた XML 文書、該当ファイルが無かった場合は null</returns>
        public XDocument? TryOpenXml(string filePath);


        /// <summary>
        /// XML ファイルを開く
        /// </summary>
        /// <param name="filePath">開くファイルの相対パス</param>
        /// <exception cref="FileNotFoundException">該当ファイルが無い</exception>
        /// <returns>開いた XML 文書</returns>
        public XDocument OpenXml(string filePath);
    }
}

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibX4.FileSystem
{
    interface IFileLoader
    {
        /// <summary>
        /// Mod等のルートディレクトリ
        /// </summary>
        public string RootDir { get; }


        /// <summary>
        /// 非同期にファイルを開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="cancellationToken">キャンセル トークン</param>
        /// <returns>ファイルのStream、該当ファイルが無かった場合はnull</returns>
        public Task<Stream?> OpenFileAsync(string filePath, CancellationToken cancellationToken = default);
    }
}

using LibX4.FileSystem;
using System.Threading;
using System.Threading.Tasks;
using X4_DataExporterWPF.Export;

namespace X4_DataExporterWPF.Internal
{
    /// <summary>
    /// サムネイル画像を管理するクラス
    /// </summary>
    internal class ThumbnailManager
    {
        /// <summary>
        /// catファイルオブジェクト
        /// </summary>
        private readonly ICatFile _CatFile;


        /// <summary>
        /// サムネの格納先フォルダ
        /// </summary>
        private readonly string _Dir;


        /// <summary>
        /// サムネイル画像が見つからなかった場合の画像データの名前
        /// </summary>
        private readonly string _NotFoundThumbnailName;


        /// <summary>
        /// サムネイル画像が見つからなかった場合の画像データ
        /// </summary>
        private byte[]? _NotFoundThumbnail;


        /// <summary>
        /// サムネイル画像を取得済みか
        /// </summary>
        private bool _IsAcquiredNotFoundThumbnail = false;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="dir">サムネの格納先フォルダ</param>
        internal ThumbnailManager(ICatFile catFile, string dir, string notFoundThumbnailName)
        {
            _CatFile = catFile;
            _Dir = dir;
            _NotFoundThumbnailName = notFoundThumbnailName;
        }


        /// <summary>
        /// 非同期にサムネイル画像を取得する
        /// </summary>
        /// <param name="macroName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<byte[]?> GetThumbnailAsync(string macroName, CancellationToken cancellationToken)
        {
            var ret = await Util.DDS2PngAsync(_CatFile, _Dir, macroName, cancellationToken);
            if (ret is not null)
            {
                return ret;
            }

            if (!_IsAcquiredNotFoundThumbnail && _NotFoundThumbnail is null)
            {
                _NotFoundThumbnail = await Util.DDS2PngAsync(_CatFile, _Dir, _NotFoundThumbnailName, cancellationToken);
            }

            return _NotFoundThumbnail;
        }
    }
}

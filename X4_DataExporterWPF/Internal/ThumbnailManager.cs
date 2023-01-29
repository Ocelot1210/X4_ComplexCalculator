using LibX4.FileSystem;
using System.Threading;
using System.Threading.Tasks;
using X4_DataExporterWPF.Export;

namespace X4_DataExporterWPF.Internal;


/// <summary>
/// サムネイル画像を管理するクラス
/// </summary>
internal class ThumbnailManager
{
    /// <summary>
    /// catファイルオブジェクト
    /// </summary>
    private readonly ICatFile _catFile;


    /// <summary>
    /// サムネの格納先フォルダ
    /// </summary>
    private readonly string _dir;


    /// <summary>
    /// サムネイル画像が見つからなかった場合の画像データの名前
    /// </summary>
    private readonly string _notFoundThumbnailName;


    /// <summary>
    /// サムネイル画像が見つからなかった場合の画像データ
    /// </summary>
    private byte[]? _notFoundThumbnail;


    /// <summary>
    /// サムネイル画像を取得済みか
    /// </summary>
    private readonly bool _isAcquiredNotFoundThumbnail = false;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="catFile">catファイルオブジェクト</param>
    /// <param name="dir">サムネの格納先フォルダ</param>
    internal ThumbnailManager(ICatFile catFile, string dir, string notFoundThumbnailName)
    {
        _catFile = catFile;
        _dir = dir;
        _notFoundThumbnailName = notFoundThumbnailName;
    }


    /// <summary>
    /// 非同期にサムネイル画像を取得する
    /// </summary>
    /// <param name="macroName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal async Task<byte[]?> GetThumbnailAsync(string macroName, CancellationToken cancellationToken)
    {
        var ret = await Util.DDS2PngAsync(_catFile, _dir, macroName, cancellationToken);
        if (ret is not null)
        {
            return ret;
        }

        if (!_isAcquiredNotFoundThumbnail && _notFoundThumbnail is null)
        {
            _notFoundThumbnail = await Util.DDS2PngAsync(_catFile, _dir, _notFoundThumbnailName, cancellationToken);
        }

        return _notFoundThumbnail;
    }
}

using LibX4.FileSystem;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace X4_DataExporterWPF.Export;

/// <summary>
/// ユーティリティクラス
/// </summary>
public static class Util
{
    /// <summary>
    /// tagsの中からサイズIDを取得する
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    public static string GetSizeIDFromTags(string? tags)
    {
        if (string.IsNullOrEmpty(tags))
        {
            return "";
        }

        string[] sizes =
        {
            "extrasmall",
            "small",
            "medium",
            "large",
            "extralarge"
        };

        var tagArr = SplitTags(tags);
        var ret = tagArr.FirstOrDefault(x => sizes.Contains(x));
        if (!string.IsNullOrEmpty(ret))
        {
            return ret;
        }


        (string, string)[] sizeAbbrs =
        {
            ("xs", "extrasmall"),
            ("xl", "extralarge"),
            ("m",  "medium"),
            ("l",  "large"),
        };

        foreach (var t in tagArr.SelectMany(x => x.Split("_")))
        {
            var size = sizeAbbrs.FirstOrDefault(x => x.Item1 == t);
            if (!string.IsNullOrEmpty(size.Item1))
            {
                return size.Item2;
            }
        }

        return "";
    }


    /// <summary>
    /// tagsを分割する
    /// </summary>
    /// <param name="tags"></param>
    /// <returns>分割結果</returns>
    public static string[] SplitTags(string? tags)
    {
        if (string.IsNullOrEmpty(tags))
        {
            return Array.Empty<string>();
        }

        // たまにtagsが[]で囲われてる場合があるため[]を削除してからSplitする
        return tags.TrimStart('[').TrimEnd(']').Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
    }


    /// <summary>
    /// 指定されたパスのDDSファイルをPNGのバイト配列として取得する
    /// </summary>
    /// <param name="catFile">catファイルオブジェクト</param>
    /// <param name="dir">フォルダパス</param>
    /// <param name="fileName">gzipで圧縮された画像のファイル名(拡張子は除く)</param>
    /// <returns></returns>
    public static async Task<byte[]?> DDS2PngAsync(ICatFile catFile, string dir, string? fileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            // 画像なし
            return null;
        }

        try
        {
            using var rawStream = await catFile.TryOpenFileAsync(Path.Combine(dir, $"{fileName}.gz"), cancellationToken);
            if (rawStream is not null)
            {
                switch (JudgeFormat(rawStream))
                {
                    case PictureFormat.Gzip:
                        {
                            using var inStream = new GZipStream(rawStream, CompressionMode.Decompress);
                            return await DDS2PngMainAsync(inStream, cancellationToken);
                        }

                    case PictureFormat.DDS:
                        return await DDS2PngMainAsync(rawStream, cancellationToken);

                    default:
                        break;
                }
            }
        }
        catch
        {
        }

        return null;
    }

    enum PictureFormat
    {
        Unknown = 0,
        Gzip = 1,
        DDS = 2
    }

    /// <summary>
    /// <paramref name="stream"/> の内容からフォーマットを判定する
    /// </summary>
    /// <param name="stream">判定対象のデータを表す <see cref="Stream"/></param>
    /// <returns>
    /// <para>gzip の場合 1</para>
    /// <para>DDS の場合 2</para>
    /// <para>それ以外の場合 -1</para>
    /// </returns>
    private static PictureFormat JudgeFormat(Stream stream)
    {
        Span<byte> buff = stackalloc byte[4];
        stream.Read(buff);
        stream.Position = 0;

        if (buff[0] == 0x1F && buff[1] == 0x8B)
        {
            return PictureFormat.Gzip;
        }

        if (buff[0] == 0x44 && buff[1] == 0x44 && buff[2] == 0x53 && buff[3] == 0x20)
        {
            return PictureFormat.DDS;
        }

        return PictureFormat.Unknown;
    }


    /// <summary>
    /// DDSファイルのStreamをPNGのバイト配列に変換する
    /// </summary>
    /// <param name="stream">変換対象のDDSファイルのStream</param>
    /// <returns>バイト配列</returns>
    private static async Task<byte[]?> DDS2PngMainAsync(Stream stream, CancellationToken cancellationToken)
    {
        var image = Pfim.Dds.Create(stream, new Pfim.PfimConfig());
        if (image.Compressed)
        {
            image.Decompress();
        }

        if (image.Format == Pfim.ImageFormat.Rgba32)
        {
            var img = Image.LoadPixelData<Bgra32>(image.Data, image.Width, image.Height);
            using var ms = new MemoryStream();

            await img.SaveAsPngAsync(ms, cancellationToken);

            return ms.ToArray();
        }
        else if (image.Format == Pfim.ImageFormat.Rgb24)
        {
            var img = Image.LoadPixelData<Bgr24>(image.Data, image.Width, image.Height);
            using var ms = new MemoryStream();

            await img.SaveAsPngAsync(ms, cancellationToken);

            return ms.ToArray();
        }

        return null;
    }
}

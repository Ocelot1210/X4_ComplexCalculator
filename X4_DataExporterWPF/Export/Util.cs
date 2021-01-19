using LibX4.FileSystem;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace X4_DataExporterWPF.Export
{
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
        /// 指定されたパスのgzipで圧縮されたDDSファイルをPNGのバイト配列として取得する
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="dir">フォルダパス</param>
        /// <param name="fileName">gzipで圧縮された画像のファイル名(拡張子は除く)</param>
        /// <returns></returns>
        public static byte[]? GzDds2Png(ICatFile catFile, string dir, string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                // 画像なし
                return null;
            }

            using var rawIconStream = catFile.TryOpenFile(Path.Combine(dir, $"{fileName}.gz"));
            if (rawIconStream is not null)
            {
                var inStream = new GZipStream(rawIconStream, CompressionMode.Decompress);
                return DDS2Png(inStream);
            }

            return null;
        }


        /// <summary>
        /// DDSファイルのStreamをPNGのバイト配列に変換する
        /// </summary>
        /// <param name="stream">変換対象のDDSファイルのStream</param>
        /// <returns>バイト配列</returns>
        private static byte[]? DDS2Png(Stream stream)
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
                img.SaveAsPng(ms);
                return ms.ToArray();
            }
            else if (image.Format == Pfim.ImageFormat.Rgb24)
            {
                var img = Image.LoadPixelData<Bgr24>(image.Data, image.Width, image.Height);
                using var ms = new MemoryStream();
                img.SaveAsPng(ms);
                return ms.ToArray();
            }

            return null;
        }
    }
}

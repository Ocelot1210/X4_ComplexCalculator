using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LibX4.FileSystem
{
    /// <summary>
    /// catファイル読み込み用クラス
    /// </summary>
    class CatFileLoader : IFileLoader
    {
        /// <summary>
        /// catやModファイルがあるルートディレクトリ
        /// </summary>
        public string RootDir { get; }


        /// <summary>
        /// まだロードされていないCat/Datファイルの絶対パス
        /// </summary>
        private readonly Stack<(string catFilePath, string datFilePath)> _NotLoadedPaths;


        /// <summary>
        /// ロード済みのファイルメタデータ
        /// </summary>
        private readonly Dictionary<string, CatEntry> _LoadedCatEntries
            = new Dictionary<string, CatEntry>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// catファイルのレコード分割用正規表現
        /// </summary>
        private static readonly Regex _CatFileRecordParser
            = new Regex("^(.+) ([0-9]+) ([0-9]+) ([0-9a-fA-F]+)$", RegexOptions.Compiled);


        /// <summary>
        /// 指定のディレクトリ直下のCatファイルを検索し、読み込み対象に追加する
        /// </summary>
        /// <param name="dirPath">Cat/Datファイルを探すディレクトリの絶対パス</param>
        public CatFileLoader(string dirPath)
        {
            RootDir = dirPath;

            var archivePaths = Directory.EnumerateFiles(dirPath, "*.cat")
                // ファイル名にsigを含むCatファイルは除外
                .Where(p => !Path.GetFileName(p).Contains("sig"))
                // Datファイルの存在を確認
                .Select(catFilePath =>
                {
                    var datFileName = Path.GetFileNameWithoutExtension(catFilePath) + ".dat";
                    var datFilePath = Path.Combine(dirPath, datFileName);
                    return (catFilePath, datFilePath);
                })
                .Where(x => File.Exists(x.datFilePath));
            _NotLoadedPaths = new Stack<(string, string)>(archivePaths);
        }


        /// <summary>
        /// ファイルを開く
        /// </summary>
        /// <param name="filePath">開きたいファイルのパス</param>
        /// <exception cref="IOException">読み込んだCatファイルの書式が不正な場合</exception>
        /// <returns>ファイルのMemoryStream、該当ファイルが無かった場合はnull</returns>
        public MemoryStream? OpenFile(string filePath)
        {
            var entry = GetCatEntry(filePath);
            if (entry != null)
            {
                using var fs = new FileStream(
                    entry.DatFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    entry.FileSize
                );
                var buff = new byte[entry.FileSize];
                fs.Seek(entry.Offset, SeekOrigin.Begin);
                fs.Read(buff, 0, buff.Length);

                return new MemoryStream(buff, false);
            }
            else
            {
                var path = Path.Combine(RootDir, filePath);
                if (!File.Exists(path))
                {
                    return null;
                }

                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                var buff = new byte[fs.Length];
                fs.Read(buff, 0, buff.Length);

                return new MemoryStream(buff, false);
            }
        }


        /// <summary>
        /// Catファイルから必要なファイルのメタデータを取得する
        /// </summary>
        /// <param name="searchFilePath">必要なファイルの相対パス</param>
        /// <exception cref="IOException">読み込んだCatファイルの書式が不正な場合</exception>
        /// <returns>見つかったファイルのメタデータ、該当ファイルが無かった場合はnull</returns>
        private CatEntry? GetCatEntry(string searchFilePath)
        {
            if (_LoadedCatEntries.TryGetValue(searchFilePath, out var entry)) return entry;

            while (_NotLoadedPaths.Any())
            {
                ReadNextCatFile();

                if (_LoadedCatEntries.TryGetValue(searchFilePath, out var newEntry))
                {
                    return newEntry;
                }
            }

            return null;
        }


        /// <summary>
        /// 次のCatファイルを読み込み、_LoadedCatEntriesに追加する
        /// </summary>
        /// <exception cref="IOException">読み込んだCatファイルの書式が不正な場合</exception>
        private void ReadNextCatFile()
        {
            var (catFilePath, datFilePath) = _NotLoadedPaths.Pop();
            var fileOffset = 0L;

            foreach (var line in File.ReadAllLines(catFilePath))
            {
                var matchs = _CatFileRecordParser.Matches(line).FirstOrDefault()?.Groups;
                if (matchs?.Count != 5)
                {
                    throw new IOException($"{catFilePath} is invalid format.");
                }

                var filePath = matchs[1].Value;
                var fileName = Path.GetFileName(matchs[1].Value);
                var fileSize = int.Parse(matchs[2].Value);
                var offset = fileOffset;
                fileOffset += fileSize;

                var entry = new CatEntry(datFilePath, fileName, fileSize, offset);
                _LoadedCatEntries.TryAdd(filePath, entry);
            }
        }
    }
}

﻿using LibX4.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace LibX4.FileSystem
{
    /// <summary>
    /// catファイル用ユーティリティクラス
    /// </summary>
    public class CatFile : ICatFile
    {
        #region スタティックメンバ
        /// <summary>
        /// Modのファイルパスを分割する正規表現
        /// </summary>
        private static readonly Regex _ParseModRegex
            = new(@"(extensions\/.+?)\/(.+)", RegexOptions.IgnoreCase);
        #endregion


        #region メンバ
        /// <summary>
        /// ファイル
        /// </summary>
        private readonly IReadOnlyList<IFileLoader> _FileLoaders;


        /// <summary>
        /// 読み込み済み MOD
        /// </summary>
        private readonly HashSet<string> _LoadedMods = new(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// 読み込み済み Index ファイル名
        /// </summary>
        private readonly HashSet<string> _LoadedIndex = new();


        /// <summary>
        /// 読み込み済み Index の内容
        /// </summary>
        private readonly Dictionary<string, string> _Index = new(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Mod情報配列
        /// </summary>
        private readonly IReadOnlyList<ModInfo> _ModInfo;
        #endregion


        #region プロパティ
        /// <summary>
        /// X4のバージョン
        /// </summary>
        public string Version { get; }


        /// <summary>
        /// Modが導入されているか
        /// </summary>
        public bool IsModInstalled => 0 < _LoadedMods.Count;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="gameRoot">X4インストール先ディレクトリパス</param>
        public CatFile(string gameRoot)
        {
            // X4のバージョンを取得
            {
                var versionDatPath = Path.Combine(gameRoot, "version.dat");
                if (File.Exists(versionDatPath))
                {
                    Version = File.ReadLines(versionDatPath).First();
                }
                else
                {
                    Version = "";
                }
            }


            var entensionsPath = Path.Combine(gameRoot, "extensions");

            var modDirPaths = Directory.Exists(entensionsPath)
                ? Directory.GetDirectories(entensionsPath)
                : Array.Empty<string>();

            var fileLoader = new List<CatFileLoader>(modDirPaths.Length + 1);
            var modInfos = new List<ModInfo>(modDirPaths.Length);

            fileLoader.Add(CatFileLoader.CreateFromDirectory(gameRoot));

            // ユーザフォルダにある content.xml を開く
            XDocumentEx.TryLoad(Path.Combine(X4Path.GetUserDirectory(), "content.xml"), out var userContentXml);

            foreach (var modDirPath in modDirPaths)
            {
                // 無効化された/無効な Mod なら読み込まないようにする
                var modInfo = new ModInfo(userContentXml, modDirPath);
                if (!modInfo.Enabled)
                {
                    continue;
                }

                var modPath = $"extensions/{Path.GetFileName(modDirPath)}".Replace('\\', '/');

                fileLoader.Add(CatFileLoader.CreateFromDirectory(modDirPath));
                modInfos.Add(modInfo);
                _LoadedMods.Add(modPath);
            }

            _FileLoaders = fileLoader;
            _ModInfo = modInfos;
        }


        /// <inheritdoc/>
        public async Task<Stream> OpenFileAsync(string filePath, CancellationToken cancellationToken = default)
            => await TryOpenFileAsync(filePath, cancellationToken) ?? throw new FileNotFoundException(null, filePath);


        /// <inheritdoc/>
        public async Task<Stream?> TryOpenFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            filePath = PathCanonicalize(filePath);

            foreach (var loader in _FileLoaders)
            {
                var ms = await loader.OpenFileAsync(filePath, cancellationToken);
                if (ms is not null)
                {
                    return ms;
                }
            }

            return null;
        }


        /// <summary>
        /// 指定されたパスに一致するxmlファイルを全部開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>XDocumentの列挙</returns>
        public async IAsyncEnumerable<XDocument> OpenXmlFilesAsync(string filePath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach(var ms in OpenFilesAsync(filePath, cancellationToken))
            {
                XDocument? ret = null;
                try
                {
                    ret = await XDocument.LoadAsync(ms, LoadOptions.None, cancellationToken);
                }
                catch (System.Xml.XmlException)
                {
                }

                if (ret is not null)
                {
                    yield return ret;
                }
                
                ms.Dispose();
            }
        }


        /// <summary>
        /// 指定されたパスに一致するファイルを全部開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>ファイルの内容の列挙</returns>
        public async IAsyncEnumerable<Stream> OpenFilesAsync(string filePath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            filePath = PathCanonicalize(filePath);

            foreach (var loader in _FileLoaders)
            {
                var ms = await loader.OpenFileAsync(filePath, cancellationToken);
                if (ms is not null)
                {
                    yield return ms;
                }
            }
        }



        /// <inheritdoc/>
        public async Task<XDocument?> TryOpenXmlAsync(string filePath, CancellationToken cancellationToken = default)
        {
            filePath = PathCanonicalize(filePath);

            XDocument? ret = null;
            foreach (var fileLoader in _FileLoaders)
            {
                using var ms = await fileLoader.OpenFileAsync(filePath, cancellationToken);
                if (ms is null)
                {
                    continue;
                }

                try
                {
                    if (ret is null)
                    {
                        ret = await XDocument.LoadAsync(ms, LoadOptions.None, cancellationToken);
                    }
                    else
                    {
                        ret.MergeXML(await XDocument.LoadAsync(ms, LoadOptions.None, cancellationToken));
                    }
                }
                catch (System.Xml.XmlException)
                {
                    // XMLのパースに失敗した場合、何もしない扱いにする(X4の動作に合わせたつもり)
                }
            }

            return ret;
        }


        /// <inheritdoc/>
        public async Task<XDocument> OpenXmlAsync(string filePath, CancellationToken cancellationToken = default)
            => await TryOpenXmlAsync(filePath, cancellationToken) ?? throw new FileNotFoundException(filePath);


        /// <inheritdoc/>
        public async Task<XDocument> OpenIndexXmlAsync(string indexFilePath, string name, CancellationToken cancellationToken = default)
        {
            if (!_LoadedIndex.Contains(indexFilePath))
            {
                foreach (var entry in (await OpenXmlAsync(indexFilePath, cancellationToken)).XPathSelectElements("/index/entry"))
                {
                    var entryName = entry.Attribute("name")?.Value;
                    if (entryName is null) continue;

                    var entryValue = entry.Attribute("value")?.Value.Replace(@"\\", @"\");
                    if (entryValue is null) continue;

                    if (!_Index.TryAdd(entryName, entryValue)) _Index[entryName] = entryValue;
                }
                _LoadedIndex.Add(indexFilePath);
            }

            var path = _Index.GetValueOrDefault(name) ?? throw new FileNotFoundException();

            if (path is null) throw new FileNotFoundException();

            // 拡張子が設定されていない場合、xml をデフォルトにする
            if (string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = $"{path}.xml";
            }

            if (path[0] == '/' || path[0] == '\\')
            {
                path = path[1..];
            }

            return await OpenXmlAsync(path, cancellationToken);
        }


        /// <summary>
        /// Modの情報をダンプする
        /// </summary>
        /// <param name="sw">ダンプ先ストリーム</param>
        public void DumpModInfo(StreamWriter sw)
        {
            if (!IsModInstalled)
            {
                return;
            }

            sw.WriteLine("ID\tName\tAuthor\tVersion\tDate\tEnabled\tSave");

            foreach (var info in _ModInfo)
            {
                sw.WriteLine($"{info.ID}\t{info.Name}\t{info.Author}\t{info.Version}\t{info.Date}\t{info.Enabled}\t{info.Save}");
            }
        }


        /// <summary>
        /// Modのファイルパス等を正規化する
        /// </summary>
        /// <param name="path">正規化対象ファイルパス</param>
        /// <returns>正規化後のファイルパス</returns>
        /// <remarks>
        /// "extensions/hogeMod/assets/～～～～"
        /// ↓
        /// "assets/～～～～"
        /// </remarks>
        private string PathCanonicalize(string path)
        {
            path = path.Replace('\\', '/');

            var modPath = _ParseModRegex.Match(path).Groups[1].Value;

            return _LoadedMods.Contains(modPath)
                ? path[(modPath.Length + 1)..]
                : path;
        }
    }
}

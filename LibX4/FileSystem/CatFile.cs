using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

            var modPaths = Directory.Exists(entensionsPath)
                ? Directory.GetDirectories(entensionsPath)
                : new string[0];

            var fileLoader = new List<CatFileLoader>(modPaths.Length + 1);
            var modInfos = new List<ModInfo>(modPaths.Length);

            fileLoader.Add(CatFileLoader.CreateFromDirectory(gameRoot));

            foreach (var path in modPaths)
            {
                // content.xmlが存在するフォルダのみ読み込む
                if (!File.Exists(Path.Combine(path, "content.xml"))) continue;

                var modPath = $"extensions/{Path.GetFileName(path)}".Replace('\\', '/');

                fileLoader.Add(CatFileLoader.CreateFromDirectory(path));
                modInfos.Add(new ModInfo(path));
                _LoadedMods.Add(modPath);
            }

            _FileLoaders = fileLoader;
            _ModInfo = modInfos;
        }


        /// <inheritdoc/>
        public Stream OpenFile(string filePath)
            => TryOpenFile(filePath) ?? throw new FileNotFoundException(null, filePath);


        /// <inheritdoc/>
        public Stream? TryOpenFile(string filePath)
        {
            filePath = PathCanonicalize(filePath);

            foreach (var loader in _FileLoaders)
            {
                var ms = loader.OpenFile(filePath);
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
        public IEnumerable<XDocument> OpenXmlFiles(string filePath)
        {
            foreach (var ms in OpenFiles(filePath))
            {
                yield return XDocument.Load(ms);
                ms.Dispose();
            }
        }


        /// <summary>
        /// 指定されたパスに一致するファイルを全部開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>ファイルの内容の列挙</returns>
        public IEnumerable<Stream> OpenFiles(string filePath)
        {
            filePath = PathCanonicalize(filePath);

            foreach (var loader in _FileLoaders)
            {
                var ms = loader.OpenFile(filePath);
                if (ms is not null)
                {
                    yield return ms;
                }
            }
        }



        /// <inheritdoc/>
        public XDocument? TryOpenXml(string filePath)
        {
            filePath = PathCanonicalize(filePath);

            XDocument? ret = null;
            foreach (var fileLoader in _FileLoaders)
            {
                using var ms = fileLoader.OpenFile(filePath);
                if (ms == null)
                {
                    continue;
                }

                if (ret == null)
                {
                    ret = XDocument.Load(ms);
                }
                else
                {
                    ret.MergeXML(XDocument.Load(ms));
                }
            }

            return ret;
        }


        /// <inheritdoc/>
        public XDocument OpenXml(string filePath)
            => TryOpenXml(filePath) ?? throw new FileNotFoundException(filePath);


        /// <inheritdoc/>
        public XDocument OpenIndexXml(string indexFilePath, string name)
        {
            if (!_LoadedIndex.Contains(indexFilePath))
            {
                foreach (var entry in OpenXml(indexFilePath).XPathSelectElements("/index/entry"))
                {
                    var entryName = entry.Attribute("name")?.Value;
                    if (entryName == null) continue;
                    var entryValue = entry.Attribute("value")?.Value.Replace(@"\\", @"\");
                    if (entryValue == null) continue;
                    if (!_Index.TryAdd(entryName, entryValue)) _Index[entryName] = entryValue;
                }
                _LoadedIndex.Add(indexFilePath);
            }

            var path = _Index.GetValueOrDefault(name) ?? throw new FileNotFoundException();

            if (path == null) throw new FileNotFoundException();

            return OpenXml($"{path}.xml");
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
                ? path.Substring(modPath.Length + 1)
                : path;
        }
    }
}

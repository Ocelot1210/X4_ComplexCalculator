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
    public class CatFile : IIndexResolver
    {
        #region スタティックメンバ
        /// <summary>
        /// Modのファイルパスを分割する正規表現
        /// </summary>
        private static readonly Regex _ParseModRegex
            = new Regex(@"(extensions\/.+?)\/(.+)", RegexOptions.IgnoreCase);
        #endregion


        #region メンバ
        /// <summary>
        /// ファイル
        /// </summary>
        private readonly IReadOnlyList<IFileLoader> _FileLoaders;


        /// <summary>
        /// 読み込み済み MOD
        /// </summary>
        private readonly HashSet<string> _LoadedMods 
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Indexファイル
        /// </summary>
        private readonly Dictionary<string, XDocument> _IndexFiles = new Dictionary<string, XDocument>();


        /// <summary>
        /// Mod情報配列
        /// </summary>
        private readonly IReadOnlyList<ModInfo> _ModInfo;
        #endregion

        #region プロパティ
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
            var entensionsPath = Path.Combine(gameRoot, "extensions");

            var modPaths = Directory.Exists(entensionsPath)
                ? Directory.GetDirectories(entensionsPath)
                : new string[0];

            var fileLoader = new List<CatFileLoader>(modPaths.Length + 1);
            var modInfos = new List<ModInfo>(modPaths.Length);

            fileLoader.Add(new CatFileLoader(gameRoot));

            foreach (var path in modPaths)
            {
                // content.xmlが存在するフォルダのみ読み込む
                if (!File.Exists(Path.Combine(path, "content.xml"))) continue;

                var modPath = $"extensions/{Path.GetFileName(path)}".Replace('\\', '/');

                fileLoader.Add(new CatFileLoader(path));
                modInfos.Add(new ModInfo(path));
                _LoadedMods.Add(modPath);
            }

            _FileLoaders = fileLoader;
            _ModInfo = modInfos;
        }


        /// <summary>
        /// 指定したファイルを開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>ファイルの内容</returns>
        public MemoryStream OpenFile(string filePath)
            => _FileLoaders
                .Select(fileLoader => fileLoader.OpenFile(filePath))
                .FirstOrDefault() ?? throw new FileNotFoundException(nameof(filePath));


        /// <summary>
        /// XML ファイルの読み込みを試みる
        /// </summary>
        /// <param name="filePath">開くファイルの相対パス</param>
        /// <returns>開いた XML 文書、該当ファイルが無かった場合は null</returns>
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


        /// <summary>
        /// XML ファイルを開く
        /// </summary>
        /// <param name="filePath">開くファイルの相対パス</param>
        /// <exception cref="FileNotFoundException">該当ファイルが無い</exception>
        /// <returns>開いた XML 文書</returns>
        public XDocument OpenXml(string filePath)
            => TryOpenXml(filePath) ?? throw new FileNotFoundException(filePath);


        /// <summary>
        /// indexファイルに記載されているxmlを開く
        /// </summary>
        /// <param name="indexFilePath">indexファイルパス</param>
        /// <param name="name">マクロ名等</param>
        /// <exception cref="FileNotFoundException">インデックスファイルに該当する名前が記載されていない場合</exception>
        /// <returns>解決結果先のファイル</returns>
        public XDocument OpenIndexXml(string indexFilePath, string name)
        {
            if (!_IndexFiles.ContainsKey(indexFilePath))
            {
                _IndexFiles.Add(indexFilePath, OpenXml(indexFilePath));
            }

            var path = _IndexFiles[indexFilePath].XPathSelectElement($"/index/entry[@name='{name}']")?.Attribute("value").Value;

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

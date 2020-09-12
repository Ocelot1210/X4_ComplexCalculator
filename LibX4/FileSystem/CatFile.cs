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
        private static readonly Regex _ParseModRegex = new Regex(@"(extensions\/.+?)\/(.+)");
        #endregion


        #region メンバ
        /// <summary>
        /// バニラのファイル
        /// </summary>
        private readonly IFileLoader _VanillaFile;


        /// <summary>
        /// Modのファイル
        /// </summary>
        private readonly Dictionary<string, IFileLoader> _ModFiles = new Dictionary<string, IFileLoader>();


        /// <summary>
        /// Indexファイル
        /// </summary>
        private readonly Dictionary<string, XDocument> _IndexFiles = new Dictionary<string, XDocument>();


        /// <summary>
        /// Mod情報配列
        /// </summary>
        private readonly ModInfo[] _ModInfo;
        #endregion

        #region プロパティ
        /// <summary>
        /// Modが導入されているか
        /// </summary>
        public bool IsModInstalled => _ModInfo.Any();
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="gameRoot">X4インストール先ディレクトリパス</param>
        public CatFile(string gameRoot)
        {
            _VanillaFile = new CatFileLoader(gameRoot);

            var modInfo = new List<ModInfo>();

            // Modのフォルダを読み込み
            if (Directory.Exists(Path.Combine(gameRoot, "extensions")))
            {
                foreach (var path in Directory.GetDirectories(Path.Combine(gameRoot, "extensions")))
                {
                    var modPath = $"extensions/{Path.GetFileName(path)}".ToLower().Replace('\\', '/');

                    // content.xmlが存在するフォルダのみ読み込む
                    if (File.Exists(Path.Combine(gameRoot, modPath, "content.xml")))
                    {
                        modInfo.Add(new ModInfo(Path.Combine(gameRoot, modPath)));
                        _ModFiles.Add(modPath, new CatFileLoader(path));
                    }
                }
            }

            _ModInfo = modInfo.ToArray();
        }


        /// <summary>
        /// 指定したファイルを開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>ファイルの内容</returns>
        public MemoryStream OpenFile(string filePath)
        {
            // バニラのデータに見つかればそちらを開く
            var vanillaFile = _VanillaFile.OpenFile(filePath);
            if (vanillaFile != null) return vanillaFile;

            // バニラのデータに見つからない場合、Modのデータを探しに行く
            foreach (var fileLoader in _ModFiles.Values)
            {
                var modFile = fileLoader.OpenFile(filePath);
                if (modFile != null) return modFile;
            }

            throw new FileNotFoundException(nameof(filePath));
        }


        /// <summary>
        /// XML ファイルの読み込みを試みる
        /// </summary>
        /// <param name="filePath">開くファイルの相対パス</param>
        /// <returns>開いた XML 文書、該当ファイルが無かった場合は null</returns>
        public XDocument? TryOpenXml(string filePath)
        {
            XDocument? ret = null;

            filePath = PathCanonicalize(filePath.Replace('\\', '/'));

            // バニラのxmlを読み込み
            {
                using var ms = _VanillaFile.OpenFile(filePath);
                if (ms != null)
                {
                    ret = XDocument.Load(ms);
                }
            }

            // Modのxmlを連結
            foreach (var (modPath, fileLoader) in _ModFiles)
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
            if (!_ModInfo.Any())
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
            path = path.ToLower().Replace('\\', '/');

            var matches = _ParseModRegex.Match(path);

            foreach (var (modPath, _) in _ModFiles)
            {
                // Modフォルダから指定されたか？
                if (modPath == matches.Groups[1].Value)
                {
                    return path.Substring(modPath.Length + 1);
                }
            }

            return path;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Common;
using System.Text.RegularExpressions;
using X4_DataExporterWPF.Export;
using System.Linq;

namespace LibX4.FileSystem
{
    /// <summary>
    /// catファイル用ユーティリティクラス
    /// </summary>
    public class CatFile
    {
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
        /// XML差分適用用ユーティリティクラス
        /// </summary>
        private readonly XMLPatcher _XMLPatcher = new XMLPatcher();


        /// <summary>
        /// Modのファイルパスを分割する正規表現
        /// </summary>
        private readonly Regex _ParseModRegex = new Regex(@"(extensions\/.+?)\/(.+)");


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
            {
                using var ret = _VanillaFile.OpenFile(filePath);

                // バニラのデータに見つかればそちらを開く
                if (ret != null)
                {
                    return ret;
                }
            }

            // バニラのデータに見つからない場合、Modのデータを探しに行く
            foreach (var fileLoader in _ModFiles.Values)
            {
                using var ret = fileLoader.OpenFile(filePath);

                if (ret != null)
                {
                    return ret;
                }
            }

            throw new FileNotFoundException(nameof(filePath));
        }


        /// <summary>
        /// xmlファイルを開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>xmlオブジェクト</returns>
        public XDocument OpenXml(string filePath)
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
                    _XMLPatcher.MergeXML(ret, XDocument.Load(ms));
                }
            }

            return ret ?? throw new FileNotFoundException(filePath);
        }


        /// <summary>
        /// 言語ファイルを開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>xmlオブジェクト</returns>
        public XDocument OpenLangXml(string filePath)
        {
            XDocument? ret = null;

            // バニラのxmlを読み込み
            {
                using var ms = _VanillaFile.OpenFile(filePath);
                if (ms != null)
                {
                    ret = XDocument.Load(ms);
                }
            }

            // Modのxmlを連結
            foreach (var loader in _ModFiles.Values)
            {
                ConcatModLangFile(loader, "t/0001.xml", ref ret);
                ConcatModLangFile(loader, filePath, ref ret);
            }

            return ret ?? throw new FileNotFoundException(filePath);
        }


        /// <summary>
        /// Modの言語ファイルを連結
        /// </summary>
        /// <param name="stream">ファイルストリーム</param>
        /// <param name="ret">言語 XDocument</param>
        private void ConcatModLangFile(IFileLoader loader, string filePath, ref XDocument? ret)
        {
            using var ms = loader.OpenFile(filePath);

            // 言語ファイルが無ければ何もしない
            if (ms == null)
            {
                return;
            }

            // バニラに言語ファイルが無いか？
            if (ret == null)
            {
                // バニラに言語ファイルが無い場合
                ret = XDocument.Load(ms);
            }
            else
            {
                // バニラに言語ファイルが有る場合、Modの言語ファイルを連結
                var src = XDocument.Load(ms);

                // Modの言語ファイルは差分ファイルか？
                if (src.Root.Name.LocalName == "diff")
                {
                    // 差分ファイルの場合、パッチ処理に差分を適用させる
                    _XMLPatcher.MergeXML(ret, src);
                }
                else
                {
                    // 差分ファイルでない場合、xmlの内容を連結する

                    foreach (var elm in src.XPathSelectElements("language/page"))
                    {
                        var addTarget = ret.XPathSelectElement($"/language/page[@id='{elm.Attribute("id").Value}']");

                        if (addTarget != null)
                        {
                            addTarget.Add(elm.Elements());
                        }
                        else
                        {
                            ret.XPathSelectElement("/language").Add(elm);
                        }
                    }
                }
            }
        }



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

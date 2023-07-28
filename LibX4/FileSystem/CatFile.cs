using LibX4.Xml;
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

namespace LibX4.FileSystem;

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
    private readonly IReadOnlyList<IFileLoader> _fileLoaders;


    /// <summary>
    /// 読み込み済み MOD
    /// </summary>
    private readonly HashSet<string> _loadedMods = new(StringComparer.OrdinalIgnoreCase);


    /// <summary>
    /// 読み込み済み Index ファイル名
    /// </summary>
    private readonly HashSet<string> _loadedIndex = new();


    /// <summary>
    /// 読み込み済み Index の内容
    /// </summary>
    private readonly Dictionary<string, string> _index = new(StringComparer.OrdinalIgnoreCase);


    /// <summary>
    /// Mod情報配列
    /// </summary>
    private readonly IReadOnlyList<ModInfo> _modInfo;
    #endregion


    #region プロパティ
    /// <summary>
    /// X4のバージョン
    /// </summary>
    public string Version { get; }


    /// <summary>
    /// Modが導入されているか
    /// </summary>
    public bool IsModInstalled => 0 < _loadedMods.Count;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="gameRoot">X4インストール先ディレクトリパス</param>
    /// <param name="option">cat ファイルの読み込みオプション</param>
    /// <exception cref="DependencyResolutionException">Mod の依存関係の解決に失敗した場合</exception>
    public CatFile(string gameRoot, CatLoadOption option = CatLoadOption.All)
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

        var modInfo = GetModInfo(gameRoot, option);

        _loadedMods = new HashSet<string>(modInfo.Select(x => $"extensions/{Path.GetFileName(x.Directory)}".Replace('\\', '/')));

        var fileLoader = new List<CatFileLoader>(modInfo.Count + 1)
        {
            CatFileLoader.CreateFromDirectory(gameRoot)
        };
        fileLoader.AddRange(modInfo.Select(x => CatFileLoader.CreateFromDirectory(x.Directory)));
        _fileLoaders = fileLoader;

        _modInfo = modInfo;
    }


    /// <summary>
    /// Mod の情報を <see cref="IReadOnlyList{ModInfo}"/> で返す
    /// </summary>
    /// <param name="gameRoot">X4 インストール先ディレクトリパス</param>
    /// <param name="option">cat ファイルの読み込みオプション</param>
    /// <returns>Mod の情報を表す <see cref="IReadOnlyList{ModInfo}"/></returns>
    private static IReadOnlyList<ModInfo> GetModInfo(string gameRoot, CatLoadOption option)
    {
        var entensionsPath = Path.Combine(gameRoot, "extensions");

        // extensions フォルダが無い場合、Mod が無いと見なす
        if (!Directory.Exists(entensionsPath))
        {
            return new List<ModInfo>();
        }

        // ユーザフォルダにある content.xml を開く
        _ = XDocumentEx.TryLoad(Path.Combine(X4Path.GetUserDirectory(), "content.xml"), out var userContentXml);

        var unloadedMods = Directory.GetDirectories(entensionsPath)
            .Select(x => new ModInfo(userContentXml, x))
            .Where(x => x.CanLoad(option))
            .OrderBy(x => x.Name)
            .ToList();

        // Mod の依存関係情報に Mod 情報を設定する
        foreach (var mod in unloadedMods)
        {
            mod.SetModInfoToDependencies(unloadedMods);
        }

        var modInfos = new List<ModInfo>(unloadedMods.Count);

        while (unloadedMods.Any())
        {
            var prevUnloadModsCount = unloadedMods.Count;
            for (var i = 0; i < unloadedMods.Count; i++)
            {
                var modInfo = unloadedMods[i];

                // 必須 Mod がロード済みかつ任意 Mod が未ロードでないか？
                var dependencies = modInfo.Dependencies;
                if (dependencies.Where(x => !x.Optional).All(x => modInfos.Any(y => x.ID == y.ID)) &&
                    !dependencies.Where(x => x.Optional).Any(x => unloadedMods.Any(y => x.ID == y.ID)))
                {
                    modInfos.Add(modInfo);
                    unloadedMods.RemoveAt(i);
                    i--;
                }
            }

            // 未ロードの Mod 数に変化が無ければ依存関係を満たせていないと見なす
            if (prevUnloadModsCount == unloadedMods.Count)
            {
                throw new DependencyResolutionException("Failed to resolve Mod dependencies.", unloadedMods);
            }
        }

        return modInfos;
    }


    /// <inheritdoc/>
    public async Task<Stream> OpenFileAsync(string filePath, CancellationToken cancellationToken = default)
        => await TryOpenFileAsync(filePath, cancellationToken) ?? throw new FileNotFoundException(null, filePath);


    /// <inheritdoc/>
    public async Task<Stream?> TryOpenFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        filePath = PathCanonicalize(filePath);

        foreach (var loader in _fileLoaders)
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

        foreach (var loader in _fileLoaders)
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
        foreach (var fileLoader in _fileLoaders)
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
        if (!_loadedIndex.Contains(indexFilePath))
        {
            foreach (var entry in (await OpenXmlAsync(indexFilePath, cancellationToken)).XPathSelectElements("/index/entry"))
            {
                var entryName = entry.Attribute("name")?.Value;
                if (entryName is null) continue;

                var entryValue = entry.Attribute("value")?.Value.Replace(@"\\", @"\");
                if (entryValue is null) continue;

                if (!_index.TryAdd(entryName, entryValue)) _index[entryName] = entryValue;
            }
            _loadedIndex.Add(indexFilePath);
        }

        var path = _index.GetValueOrDefault(name) ?? throw new FileNotFoundException();

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

        foreach (var info in _modInfo)
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
        path = path.Replace('\\', '/').ToLower();

        var modPath = _ParseModRegex.Match(path).Groups[1].Value;

        return _loadedMods.Contains(modPath)
            ? path[(modPath.Length + 1)..]
            : path;
    }
}

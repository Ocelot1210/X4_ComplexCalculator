using Microsoft.IO;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibX4.FileSystem;

/// <summary>
/// catファイル読み込み用クラス
/// </summary>
sealed class CatFileLoader : IFileLoader
{
    #region layout
    /// <summary>
    /// catやModファイルがあるルートディレクトリ
    /// </summary>
    public string RootDir { get; }


    /// <summary>
    /// ファイルメタデータ
    /// </summary>
    private readonly Dictionary<string, Entry> _entries;


    /// <summary>
    /// dat ファイルから <see cref="RecyclableMemoryStream"/> を作成する際に使用する <see cref="RecyclableMemoryStreamManager"/>
    /// </summary>
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;


    /// <summary>
    /// catファイルの1レコード分の情報
    /// </summary>
    /// <param name="datFilePath">ファイルの実体があるdatファイルパス</param>
    /// <param name="filename">ファイル名</param>
    /// <param name="fileSize">ファイルサイズ</param>
    /// <param name="offset">datファイル上のオフセット</param>
    sealed record Entry(string DatFilePath, string FileName, int FileSize, long Offset);
    #endregion


    #region factory
    /// <summary>
    /// 指定の初期値で <see cref="CatFileLoader"/> を生成する
    /// </summary>
    /// <param name="rootDir">catやModファイルがあるルートディレクトリ</param>
    /// <param name="capacity">格納できるファイルメタデータ数の初期値</param>
    private CatFileLoader(string rootDir, int capacity)
    {
        Debug.Assert(rootDir is not null);

        RootDir = rootDir;
        _entries = new Dictionary<string, Entry>(capacity, StringComparer.OrdinalIgnoreCase);
        _memoryStreamManager = new RecyclableMemoryStreamManager();
    }


    /// <summary>
    /// 指定のディレクトリ直下のCatファイルを読み込み、<see cref="CatFileLoader"/> を生成する
    /// </summary>
    /// <param name="rootDir">Cat/Datファイルを探すディレクトリの絶対パス</param>
    /// <returns>指定のディレクトリ直下のCatファイルを読み込んだ<see cref="CatFileLoader"/></returns>
    public static CatFileLoader CreateFromDirectory(string rootDir)
        => Create(rootDir, Directory.GetFiles(rootDir, "*.cat"));


    /// <summary>
    /// 指定のCatファイルを読み込み、<see cref="CatFileLoader"/> を生成する
    /// </summary>
    /// <param name="rootDir">Cat/Datファイルの存在するルートディレクトリの絶対パス</param>
    /// <param name="catPaths">読み込むCatファイルの絶対パスの配列</param>
    /// <returns>指定のCatファイルを読み込んだ<see cref="CatFileLoader"/></returns>
    public static CatFileLoader Create(string rootDir, params string[] catPaths)
    {
        // キャパシティ計算
        int capacity = 0;
        var cats = new Stack<(string DatPath, byte[] Bytes)>(catPaths.Length);
        foreach (var catPath in catPaths)
        {
            // ファイル名にsigを含むCatファイルは除外
            if (Path.GetFileName(catPath).Contains("sig")) continue;

            // Datファイルの存在を確認
            var datPath = Path.ChangeExtension(catPath, "dat");
            if (!File.Exists(datPath)) continue;

            // 行数カウント
            byte[] bytes = File.ReadAllBytes(catPath);
            for (int j = 0; j < bytes.Length; j++) if (bytes[j] == (byte)'\n') capacity++;
            cats.Push((datPath, bytes));
        }

        // パース
        var loader = new CatFileLoader(rootDir, capacity);
        while (cats.TryPop(out var cat)) loader.Parse(cat.DatPath, cat.Bytes);
        return loader;
    }


    /// <summary>
    /// 指定のCatデータを読み込む
    /// </summary>
    /// <param name="datPath">Catデータに対応するDatファイルへの絶対パス</param>
    /// <param name="bytes">Catデータ</param>
    private void Parse(string datPath, byte[] bytes)
    {
        Debug.Assert(datPath is not null);
        Debug.Assert(bytes is not null);

        // その行に出現したスペースの位置をスタック形式で記憶
        Span<int> stack = stackalloc int[8];
        int ptr = 0;

        int lineStart = 0;
        long fileOffset = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            switch (bytes[i])
            {
                case (byte)' ': // 0x20
                    if (stack.Length <= ptr) // スタックがあふれる場合、最新3つ以外削除
                    {
                        stack[^3..].CopyTo(stack);
                        ptr = 3;
                    }
                    stack[ptr++] = i;
                    continue;

                case (byte)'\n': // 0x0A
                    // directory/file name.txt 123000 789000 00000000000000000000000000000000
                    //               ^        ^      ^      ^
                    //                        s

                    if (ptr < 3) Throw("Invalid format.");  // 一行につきスペースは最低3つ
                    int s = stack[ptr - 3];
                    string filePath = Encoding.UTF8.GetString(bytes.AsSpan()[lineStart..s]);
                    bool success = Utf8Parser.TryParse(bytes.AsSpan(s + 1), out int fileSize, out _);
                    if (!success) Throw("File size must be an integer.");
                    if (!_entries.ContainsKey(filePath))
                    {
                        string fileName = Path.GetFileName(filePath);
                        _entries.Add(filePath, new(datPath, fileName, fileSize, fileOffset));
                    }
                    ptr = 0;
                    lineStart = i + 1;
                    fileOffset += fileSize;
                    continue;

                // opcode switch
                case 0x0B or 0x0C or 0x0D or 0x0E or 0x0F
                  or 0x10 or 0x11 or 0x12 or 0x13 or 0x14 or 0x15 or 0x16 or 0x17
                  or 0x18 or 0x19 or 0x1A or 0x1B or 0x1C or 0x1D or 0x1E or 0x1F:
                    continue;
            }
        }
    }


    /// <summary>
    /// <see cref="XrCatalogParseException"/> をスローする
    /// </summary>
    [DoesNotReturn]
    private static void Throw(string message)
    {
        throw new XrCatalogParseException(message);
    }
    #endregion


    #region open
    /// <summary>
    /// ファイルを開く
    /// </summary>
    /// <param name="filePath">開きたいファイルのパス</param>
    /// <exception cref="IOException">読み込んだCatファイルの書式が不正な場合</exception>
    /// <returns>ファイルのMemoryStream、該当ファイルが無かった場合はnull</returns>
    public async Task<Stream?> OpenFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (_entries.TryGetValue(filePath, out var entry))
        {
            // ファイルサイズが空なら空の MemoryStream を返す
            if (entry.FileSize == 0)
            {
                return _memoryStreamManager.GetStream();
            }

            using var fs = new FileStream(
                entry.DatFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                entry.FileSize
            );
            fs.Seek(entry.Offset, SeekOrigin.Begin);

            var ret = _memoryStreamManager.GetStream(filePath, entry.FileSize);
            await fs.CopyToAsync(ret, cancellationToken);
            ret.Position = 0;
            ret.SetLength(entry.FileSize);

            return ret;
        }
        else
        {
            var path = Path.Combine(RootDir, filePath);
            if (!File.Exists(path))
            {
                return null;
            }

            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }
    }
    #endregion
}


/// <summary>
/// Catファイルの読み込み中にスローされる例外
/// </summary>
[Serializable]
public class XrCatalogParseException : Exception
{
    /// <summary>
    /// <see cref="XrCatalogParseException"/> を生成する
    /// </summary>
    public XrCatalogParseException()
    {

    }


    /// <summary>
    /// <see cref="XrCatalogParseException"/> を生成する
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    public XrCatalogParseException(string message)
        : base(message)
    {

    }


    /// <summary>
    /// <see cref="XrCatalogParseException"/> を生成する
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    /// <param name="inner">原因となる例外</param>
    public XrCatalogParseException(string message, Exception inner)
        : base(message, inner)
    {

    }


    /// <summary>
    /// <see cref="XrCatalogParseException"/> を生成する
    /// </summary>
    /// <param name="info">シリアライズ情報</param>
    /// <param name="context">ストリーミングコンテキスト</param>
    protected XrCatalogParseException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {

    }
}

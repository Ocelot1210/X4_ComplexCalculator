using LibX4.Xml;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibX4.FileSystem;

/// <summary>
/// Modの情報
/// </summary>
class ModInfo
{
    /// <summary>
    /// 識別ID
    /// </summary>
    public string ID { get; } = "";


    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; } = "";


    /// <summary>
    /// 作者
    /// </summary>
    public string Author { get; } = "";


    /// <summary>
    /// バージョン
    /// </summary>
    public string Version { get; } = "";


    /// <summary>
    /// 作成日時
    /// </summary>
    public string Date { get; } = "";


    /// <summary>
    /// 有効化されているか
    /// </summary>
    public bool Enabled { get; } = false;


    /// <summary>
    /// 保存ファイルに影響を与えるか？
    /// </summary>
    public string Save { get; } = "";


    /// <summary>
    /// Mod のフォルダパス
    /// </summary>
    public string Directory { get; } = "";


    /// <summary>
    /// この Mod が必要とする(依存する) Mod 一覧
    /// </summary>
    public IReadOnlyList<DependencyInfo> Dependencies { get; } = Array.Empty<DependencyInfo>();


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="userContentXml">X4 のユーザフォルダ内の content.xml</param>
    /// <param name="modDirPath">Modのフォルダパス(絶対パスで指定すること)</param>
    public ModInfo(XDocument? userContentXml, string modDirPath)
    {
        // Mod フォルダ内の content.xml の読み込みに失敗したら何もしない
        var contentXmlPath = Path.Combine(modDirPath, "content.xml");
        if (!XDocumentEx.TryLoad(contentXmlPath, out var modContentXml))
        {
            return;
        }

        Directory = modDirPath;

        // Mod フォルダ内の content.xml から ID を取得
        ID = modContentXml.Root?.Attribute("id")?.Value ?? "";
        if (string.IsNullOrEmpty(ID))
        {
            // ID が指定されていない場合、Mod のフォルダ名が ID になる
            ID = Path.GetFileName(modDirPath);
        }

        Name    = modContentXml.Root?.Attribute("name")?.Value    ?? "";
        Author  = modContentXml.Root?.Attribute("author")?.Value  ?? "";
        Version = modContentXml.Root?.Attribute("version")?.Value ?? "";
        Date    = modContentXml.Root?.Attribute("date")?.Value    ?? "";
        Save    = modContentXml.Root?.Attribute("save")?.Value    ?? "";
        Enabled = GetIsModEnabled(userContentXml, modContentXml);
        Dependencies = DependencyInfo.CreateFromContentXml(modContentXml);
    }


    /// <summary>
    /// Mod を読み込めるか判定する
    /// </summary>
    /// <param name="option">cat ファイルの読み込みオプション</param>
    /// <returns>Mod を読み込んでも良い場合、<c>true;</c> それ以外の場合 <c>false;</c></returns>
    /// <exception cref="NotImplementedException"><paramref name="option"/>が無効な場合</exception>
    public bool CanLoad(CatLoadOption option)
    {
        return Enabled && option switch
        {
            CatLoadOption.All => true,
            CatLoadOption.Official => Author == "Egosoft GmbH",
            CatLoadOption.Vanilla => false,
            _ => throw new NotImplementedException()
        };
    }


    /// <summary>
    /// Mod が有効化されているかを取得する
    /// </summary>
    /// <param name="userContentXml">X4 のユーザフォルダ内の content.xml</param>
    /// <param name="modContentXml">Mod フォルダ内の content.xml</param>
    /// <returns></returns>
    private bool GetIsModEnabled(XDocument? userContentXml, XDocument modContentXml)
    {
        var modElm = userContentXml?.Root?.XPathSelectElement($"extension[@id='{ID}']");
        if (modElm is null)
        {
            var enabledText = modContentXml.Root?.Attribute("enabled")?.Value ?? "";
            return X4StrToBool(enabledText, true);
        }
        else
        {
            var enabledText = modElm.Attribute("enabled")?.Value ?? "";
            return X4StrToBool(enabledText, true);
        }
    }


    /// <summary>
    /// X4 で使用される bool 値を表す文字列を <see cref="bool"/> に変換する
    /// </summary>
    /// <param name="str">変換対象の文字列</param>
    /// <param name="defaultValue">変換後の文字列</param>
    /// <returns><paramref name="str"/> に対応する bool 値</returns>
    private static bool X4StrToBool(string str, bool defaultValue)
    {
        if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return (int)result != 0;
        }

        if (str.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (str.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return defaultValue;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace LibX4.FileSystem;


/// <summary>
/// Mod の依存関係を表すクラス
/// </summary>
public class DependencyInfo
{
    /// <summary>
    /// Mod の ID
    /// </summary>
    public string ID { get; }

    /// <summary>
    /// 任意な Mod か
    /// </summary>
    public bool Optional { get; }

    /// <summary>
    /// Mod の名前
    /// </summary>
    public string Name { get; }


    /// <summary>
    /// Mod の詳細情報
    /// </summary>
    private ModInfo? _modInfo = null;

    /// <summary>
    /// Mod の詳細情報
    /// </summary>
    public ModInfo? ModInfo
    {
        get => _modInfo;
        private set
        {
            if (value is not null && ID != value.ID) throw new ArgumentException("value.ID");
            _modInfo = value;
        }
    }


    /// <summary>
    /// content.xml から <see cref="DependencyInfo"/> の一覧を作成する
    /// </summary>
    /// <param name="modContentXml">content.xml を表す <see cref="XDocument"/></param>
    /// <returns>content.xml に記載されている依存関係一覧の一覧</returns>
    public static IReadOnlyList<DependencyInfo> CreateFromContentXml(XDocument modContentXml)
        => EnumerateFromContentXml(modContentXml).ToArray();


    /// <summary>
    /// content.xml から <see cref="DependencyInfo"/> の列挙を作成する
    /// </summary>
    /// <param name="modContentXml">content.xml を表す <see cref="XDocument"/></param>
    /// <returns>content.xml に記載されている依存関係一覧の列挙</returns>
    private static IEnumerable<DependencyInfo> EnumerateFromContentXml(XDocument modContentXml)
    {
        var dependencies = modContentXml.Root?.XPathSelectElements("dependency");
        if (dependencies is null) yield break;

        foreach (var dependency in dependencies)
        {
            var id = dependency.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(id)) continue;

            var optionalText = dependency.Attribute("optional")?.Value;
            var name = dependency.Attribute("name")?.Value ?? "";

            yield return new DependencyInfo(id, bool.TryParse(optionalText, out var optional) && optional, name);
        }
    }


    /// <summary>
    /// 新しいインスタンスを作成
    /// </summary>
    /// <param name="id">Mod の ID</param>
    /// <param name="optional">任意な Mod か</param>
    /// <param name="name">Mod の名前</param>
    private DependencyInfo(string id, bool optional, string name)
    {
        ID = id;
        Optional = optional;
        Name = name;
    }


    /// <summary>
    /// Mod の詳細情報を設定(初期化)する
    /// </summary>
    /// <param name="modInfo">Mod の詳細情報</param>
    public void InitModInfo(ModInfo modInfo)
    {
        if (_modInfo != null)
        {
            throw new InvalidOperationException();
        }

        ModInfo = modInfo;
    }
}

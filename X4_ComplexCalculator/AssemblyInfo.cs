using System.Reflection;
using System.Windows;
using X4_ComplexCalculator;

[assembly: AssemblyVersion(VersionInfo.BaseVersion)]
[assembly: AssemblyFileVersion(VersionInfo.BaseVersion)]
[assembly: AssemblyInformationalVersion(VersionInfo.DetailVersion)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page,
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page,
                                              // app, or any theme specific resource dictionaries)
)]


namespace X4_ComplexCalculator;

/// <summary>
/// アプリケーションのバージョン情報を提供するクラス
/// </summary>
public static class VersionInfo
{
    /// <summary>
    /// バージョン名
    /// ex: 1.4.1
    /// </summary>
    internal const string BaseVersion = ThisAssembly.Git.BaseVersion.Major + "."
        + ThisAssembly.Git.BaseVersion.Minor + "." + ThisAssembly.Git.BaseVersion.Patch;


    /// <summary>
    /// より詳細なバージョン名
    /// ex: v1.4.1-4-g306b97d+dirty (Debug)
    /// </summary>
    internal const string DetailVersion = ThisAssembly.Git.Tag == ""
        ? "Unknown version" + " (" + Config + ")"
        : ThisAssembly.Git.Tag + Dirty + " (" + Config + ")";


    /// <summary>
    /// コミットされていない変更がある場合 +dirty になる
    /// </summary>
    private const string Dirty = ThisAssembly.Git.IsDirty ? "+dirty" : "";


    /// <summary>
    /// ビルド時の構成名
    /// </summary>
#if DEBUG
    private const string Config = "Debug";
#else
    private const string Config = "Release";
#endif
}

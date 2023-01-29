using System.Reflection;
using System.Windows;
using X4_ComplexCalculator;

[assembly: AssemblyVersion(VersionInfo.BASE_VERSION)]
[assembly: AssemblyFileVersion(VersionInfo.BASE_VERSION)]
[assembly: AssemblyInformationalVersion(VersionInfo.DETAIL_VERSION)]


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
    internal const string BASE_VERSION = ThisAssembly.Git.BaseVersion.Major + "."
        + ThisAssembly.Git.BaseVersion.Minor + "." + ThisAssembly.Git.BaseVersion.Patch;


    /// <summary>
    /// より詳細なバージョン名
    /// ex: v1.4.1-4-g306b97d+dirty (Debug)
    /// </summary>
    internal const string DETAIL_VERSION = ThisAssembly.Git.Tag == ""
        ? "Unknown version" + " (" + CONFIG + ")"
        : ThisAssembly.Git.Tag + DIRTY + " (" + CONFIG + ")";


    /// <summary>
    /// コミットされていない変更がある場合 +dirty になる
    /// </summary>
    private const string DIRTY = ThisAssembly.Git.IsDirty ? "+dirty" : "";


    /// <summary>
    /// ビルド時の構成名
    /// </summary>
#if DEBUG
    private const string CONFIG = "Debug";
#else
    private const string CONFIG = "Release";
#endif
}

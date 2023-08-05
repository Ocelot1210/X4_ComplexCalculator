using System;
using System.Collections.Generic;

namespace LibX4.FileSystem;


/// <summary>
/// Mod の依存関係の解決に失敗した際にスローされる例外
/// </summary>
public class DependencyResolutionException : Exception
{
    public readonly IReadOnlyList<ModInfo> UnloadedMods = Array.Empty<ModInfo>();

    public DependencyResolutionException() { }

    public DependencyResolutionException(string message) : base(message) { }

    public DependencyResolutionException(string message, Exception? innerException)
        : base(message, innerException) { }

    public DependencyResolutionException(string message, IReadOnlyList<ModInfo> unloadedMods) : base(message)
    {
        UnloadedMods = unloadedMods;
    }
}

namespace LibX4.Lang;

/// <summary>
/// X4 の言語フィールド文字列 (例: {1001,2490}) を解決するインターフェース
/// </summary>
public interface ILanguageResolver
{
    /// <summary>
    /// 言語フィールド文字列 (例: {1001,2490}) を解決する
    /// </summary>
    /// <param name="target">言語フィールド文字列を含む文字列</param>
    /// <returns>言語フィールド文字列を解決し置き換えた文字列</returns>
    string Resolve(string target);
}

namespace LibX4.FileSystem;


/// <summary>
/// .cat ファイルを読み込むオプション
/// </summary>
public enum CatLoadOption
{
    None = 0b0000_0000,

    /// <summary>
    /// バニラのデータを読み込む
    /// </summary>
    Vanilla = 0b0000_0001,

    /// <summary>
    /// DLC のデータを読み込む
    /// </summary>
    Dlc = 0b0000_0010,

    /// <summary>
    /// Mod のデータを読み込む
    /// </summary>
    Mod = 0b0000_0100,

    /// <summary>
    /// 公式のデータを読み込む
    /// </summary>
    Official = Vanilla | Dlc,

    /// <summary>
    /// 全てのデータを読み込む
    /// </summary>
    All = Vanilla | Dlc | Mod,
}

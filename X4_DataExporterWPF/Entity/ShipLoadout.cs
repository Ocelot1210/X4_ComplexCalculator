namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 艦船のロードアウト情報
    /// </summary>
    /// <param name="shipID">艦船ID</param>
    /// <param name="loadoutID">ロードアウトID</param>
    /// <param name="macroName">マクロ名</param>
    /// <param name="groupName">グループ名</param>
    /// <param name="count">個数</param>
    public sealed record ShipLoadout(
        string ShipID,
        string LoadoutID,
        string MacroName,
        string GroupName,
        long Count
    );
}

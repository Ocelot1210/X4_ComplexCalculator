namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュール
    /// </summary>
    /// <param name="ModuleID">モジュールID</param>
    /// <param name="ModuleTypeID">モジュール種別ID</param>
    /// <param name="Macro">マクロ名</param>
    /// <param name="MaxWorkers">最大労働者数</param>
    /// <param name="WorkersCapacity">収容可能な労働者数</param>
    /// <param name="NoBlueprint">設計図有無</param>
    /// <param name="Thumbnail">サムネ画像</param>
    public sealed record Module(
        string ModuleID,
        string ModuleTypeID,
        string Macro,
        long MaxWorkers,
        long WorkersCapacity,
        bool NoBlueprint,
        byte[]? Thumbnail
    );
}

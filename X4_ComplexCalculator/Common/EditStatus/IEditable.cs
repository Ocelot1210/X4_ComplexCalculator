namespace X4_ComplexCalculator.Common.EditStatus;

/// <summary>
/// 編集状態保持用interface
/// </summary>
interface IEditable
{
    /// <summary>
    /// 編集状態
    /// </summary>
    public EditStatus EditStatus { get; set; }
}

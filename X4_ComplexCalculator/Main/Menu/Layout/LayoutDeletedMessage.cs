namespace X4_ComplexCalculator.Main.Menu.Layout;

/// <summary>
/// レイアウトが削除された時のメッセージ
/// </summary>
/// <param name="item">削除されたレイアウト</param>
public sealed class LayoutDeletedMessage(LayoutMenuItem item)
{
    /// <summary>
    /// 削除されたレイアウト
    /// </summary>
    public LayoutMenuItem Item { get; } = item;
}
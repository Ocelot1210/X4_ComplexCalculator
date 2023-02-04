using System.Windows.Forms;

namespace X4_ComplexCalculator.Common.Dialog.MessageBoxes;

internal static class DialogResultConverter
{
    /// <summary>
    /// <see cref="LocalizedMessageBoxResult"/> を <see cref="TaskDialogButton"/> に変換する
    /// </summary>
    /// <param name="result">変換元の <see cref="LocalizedMessageBoxResult"/></param>
    /// <returns>変換結果の <see cref="TaskDialogResult"/></returns>
    internal static TaskDialogButton ToTaskDialogButton(this LocalizedMessageBoxResult result)
    {
        return result switch
        {
            LocalizedMessageBoxResult.Yes       => TaskDialogButton.Yes,
            LocalizedMessageBoxResult.No        => TaskDialogButton.No,
            LocalizedMessageBoxResult.Cancel    => TaskDialogButton.Cancel,
            _                                   => TaskDialogButton.OK,
        };
    }


    /// <summary>
    ///  <see cref="TaskDialogButton"/> を <see cref="LocalizedMessageBoxResult"/> に変換する
    /// </summary>
    /// <param name="button">変換元の <see cref="TaskDialogButton"/></param>
    /// <returns>変換結果の <see cref="LocalizedMessageBoxResult"/></returns>
    internal static LocalizedMessageBoxResult ToLocalizedMessageBoxResult(this TaskDialogButton button)
    {
        // switch 式が使えない
        if (button == TaskDialogButton.OK) return LocalizedMessageBoxResult.OK;
        if (button == TaskDialogButton.Cancel) return LocalizedMessageBoxResult.Cancel;
        if (button == TaskDialogButton.Yes) return LocalizedMessageBoxResult.Yes;
        if (button == TaskDialogButton.No) return LocalizedMessageBoxResult.No;

        return LocalizedMessageBoxResult.None;
    }
}
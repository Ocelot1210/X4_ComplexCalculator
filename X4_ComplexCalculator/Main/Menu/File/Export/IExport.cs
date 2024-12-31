using CommunityToolkit.Mvvm.Input;

namespace X4_ComplexCalculator.Main.Menu.File.Export;

public interface IExport
{
    /// <summary>
    /// メニュー表示用タイトル
    /// </summary>
    public string Title { get; }


    /// <summary>
    /// Viewより呼ばれるCommand
    /// </summary>
    public IRelayCommand ExportCommand { get; }
}

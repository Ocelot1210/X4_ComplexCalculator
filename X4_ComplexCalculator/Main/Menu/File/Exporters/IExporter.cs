using CommunityToolkit.Mvvm.Input;

namespace X4_ComplexCalculator.Main.Menu.File.Exporters;

public interface IExporter
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

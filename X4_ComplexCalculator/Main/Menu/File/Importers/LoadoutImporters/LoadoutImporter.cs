using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace X4_ComplexCalculator.Main.Menu.File.Importers.LoadoutImporters;

/// <summary>
/// 装備をインポート
/// </summary>
partial class LoadoutImporter : ObservableObject, IImporter
{
    /// <summary>
    /// メニュー表示用タイトル
    /// </summary>
    public string Title => "Lang:MainWindow_Menu_File_MenuItem_Import_MenuItem_Loadout_Header";


    /// <summary>
    /// インポート処理
    /// </summary>
    [RelayCommand]
    private void Import()
    {
        SelectLoadoutDialog.ShowImportDialog();
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X4_ComplexCalculator.Common.Dialog.SelectStringDialog;

namespace X4_ComplexCalculator.Main.Menu.File.Exporters.StationCalculatorExporter;

/// <summary>
/// StationCalculator向けにステーションの情報をエクスポートする
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="workAreaManager">作業エリア管理</param>
partial class StationCalculatorExporter(WorkAreaManager workAreaManager) : ObservableObject, IExporter
{
    /// <summary>
    /// 作業エリア管理
    /// </summary>
    private readonly WorkAreaManager _workAreaManager = workAreaManager;


    /// <summary>
    /// タイトル文字列
    /// </summary>
    public string Title => "Lang:MainWindow_Menu_File_MenuItem_Import_MenuItem_StationCalculator_Header";


    /// <summary>
    /// エクスポート処理
    /// </summary>
    /// <param name="WorkArea"></param>
    /// <returns></returns>
    [RelayCommand]
    private void Export()
    {
        if (_workAreaManager.ActiveContent is null)
        {
            return;
        }

        var sb = new StringBuilder();

        sb.Append(@"http://www.x4-game.com/#/station-calculator?");

        // モジュール情報を追加
        sb.Append("l=@");


        var ignoreModuleTypeIds = new HashSet<string>() { 
            "ventureplatform",
        };

        var ignoreModuleIds = new HashSet<string>() {
            "module_gen_dock_m_venturer_01",
            "module_par_def_claim_story_01",
            "module_pir_stor_condensate_s_01",
        };

        var modules = _workAreaManager.ActiveContent.WorkArea.StationData.ModulesInfo.Modules
            .Where(x => !ignoreModuleTypeIds.Contains(x.Module.ModuleType.ModuleTypeID) && !ignoreModuleIds.Contains(x.Module.ID));

        sb.AppendJoin(";,", modules.Select(x => $"$module-{x.Module.ID},count:{x.ModuleCount}"));

        SelectStringDialog.ShowDialog("Lang:StationCalculatorExport_Title", "Lang:StationCalculatorExport_Description", sb.ToString(), hideCancelButton: true);
    }
}

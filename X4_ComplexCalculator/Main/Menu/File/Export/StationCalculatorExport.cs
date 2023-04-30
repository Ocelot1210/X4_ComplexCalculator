using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using X4_ComplexCalculator.Common.Dialog.SelectStringDialog;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main.Menu.File.Export;

/// <summary>
/// StationCalculator向けにステーションの情報をエクスポートする
/// </summary>
class StationCalculatorExport : BindableBase, IExport
{
    /// <summary>
    /// タイトル文字列
    /// </summary>
    public string Title => "Lang:MainWindow_Menu_File_MenuItem_Import_MenuItem_StationCalculator_Header";


    /// <summary>
    /// Viewより呼ばれるコマンド
    /// </summary>
    public ICommand Command { get; }


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="command">Viewより呼ばれるCommand</param>
    public StationCalculatorExport(ICommand command)
    {
        Command = command;
    }


    /// <summary>
    /// エクスポート処理
    /// </summary>
    /// <param name="WorkArea"></param>
    /// <returns></returns>
    public bool Export(IWorkArea WorkArea)
    {
        var sb = new StringBuilder();
        var exists = false;

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

        var modules = WorkArea.StationData.ModulesInfo.Modules
            .Where(x => !ignoreModuleTypeIds.Contains(x.Module.ModuleType.ModuleTypeID) && !ignoreModuleIds.Contains(x.Module.ID));

        foreach (var module in modules)
        {
            sb.Append($"$module-{module.Module.ID},count:{module.ModuleCount};,");
            exists = true;
        }

        if (exists)
        {
            sb.Length -= 2;
        }

        SelectStringDialog.ShowDialog("Lang:StationCalculatorExport_Title", "Lang:StationCalculatorExport_Description", sb.ToString(), hideCancelButton: true);

        return true;
    }
}

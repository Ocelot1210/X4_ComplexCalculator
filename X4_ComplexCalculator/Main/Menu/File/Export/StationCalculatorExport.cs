using System.Linq;
using System.Text;
using System.Windows.Input;
using Prism.Mvvm;
using X4_ComplexCalculator.Common.Dialog.SelectStringDialog;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main.Menu.File.Export
{
    /// <summary>
    /// StationCalculator向けにステーションの情報をエクスポートする
    /// </summary>
    class StationCalculatorExport : BindableBase, IExport
    {
        /// <summary>
        /// タイトル文字列
        /// </summary>
        public string Title => "Station Calculator";


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
            foreach (var module in WorkArea.StationData.Modules.Where(x => x.Module.ModuleType.ModuleTypeID != "connectionmodule" &&
                                                               x.Module.ModuleType.ModuleTypeID != "ventureplatform" &&
                                                               x.Module.ModuleID != "module_gen_dock_m_venturer_01"))
            {
                sb.Append($"$module-{module.Module.ModuleID},count:{module.ModuleCount};,");
                exists = true;
            }

            if (exists)
            {
                sb.Length -= 2;
            }

            SelectStringDialog.ShowDialog("Lang:StationCalculatorExportTitle", "Lang:StationCalculatorExportDescription", sb.ToString(), hideCancelButton: true);

            return true;
        }
    }
}

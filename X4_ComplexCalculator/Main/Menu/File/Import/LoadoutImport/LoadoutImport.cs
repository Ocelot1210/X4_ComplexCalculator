using Prism.Mvvm;
using System.Windows.Input;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main.Menu.File.Import.LoadoutImport
{
    /// <summary>
    /// 装備をインポート
    /// </summary>
    class LoadoutImport : BindableBase, IImport
    {
        /// <summary>
        /// メニュー表示用タイトル
        /// </summary>
        public string Title => "Lang:MainWindow_Menu_File_MenuItem_Import_MenuItem_Loadout_Header";


        /// <summary>
        /// Viewより呼ばれるCommand
        /// </summary>
        public ICommand Command { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="command">Viewより呼ばれるCommand</param>
        public LoadoutImport(ICommand command) => Command = command;


        /// <summary>
        /// インポート処理
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        public bool Import(IWorkArea _) => true;    // 何もしない


        /// <summary>
        /// インポート対象を選択
        /// </summary>
        /// <returns>インポート対象数</returns>
        public int Select()
        {
            SelectLoadoutDialog.ShowImportDialog();
            return 0;
        }
    }
}

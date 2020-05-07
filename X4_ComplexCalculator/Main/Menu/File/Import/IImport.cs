using System.Windows.Input;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main.Menu.File.Import
{
    interface IImport
    {
        /// <summary>
        /// メニュー表示用タイトル
        /// </summary>
        public string Title { get; }


        /// <summary>
        /// Viewより呼ばれるCommand
        /// </summary>
        public ICommand Command { get; }


        /// <summary>
        /// インポート実行
        /// </summary>
        /// <param name="workArea">作業エリア</param>
        /// <returns>インポートに成功したか</returns>
        public bool Import(IWorkArea workArea);
    }
}

using System.Windows.Input;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main.Menu.File.Export
{
    public interface IExport
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
        /// エクスポート実行
        /// </summary>
        /// <param name="WorkArea">作業エリア</param>
        /// <returns>エクスポートに成功したか</returns>
        public bool Export(IWorkArea WorkArea);
    }
}

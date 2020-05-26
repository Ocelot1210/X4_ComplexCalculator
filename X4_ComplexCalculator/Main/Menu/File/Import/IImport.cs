using System.Collections.Generic;
using System.Windows.Input;
using X4_ComplexCalculator.Main.PlanningArea;

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
        /// インポート対象を選択
        /// </summary>
        /// <returns>インポート対象数</returns>
        public int Select();


        /// <summary>
        /// インポート実行
        /// </summary>
        /// <param name="PlanningArea">作業エリア</param>
        /// <returns>インポートに成功したか</returns>
        public bool Import(IPlanningArea PlanningArea);
    }
}

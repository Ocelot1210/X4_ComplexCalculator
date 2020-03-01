using System.Windows;
using System.Windows.Data;
using X4_ComplexCalculator.Common;


namespace X4_ComplexCalculator.Main.ModulesGrid.SelectModule
{
    /// <summary>
    /// AddModuleWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectModuleWindow : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modules">モジュール追加対象</param>
        /// <param name="isReplaceMode">置換モードか(falseで複数選択許可)</param>
        public SelectModuleWindow(SmartCollection<ModulesGridItem> modules, bool isReplaceMode)
        {
            InitializeComponent();

            var viewModel = new SelectModuleViewModel(modules, isReplaceMode, (CollectionViewSource)Resources["ModulesViewSource"]);
            Closing += viewModel.OnWindowClosing;

            DataContext = viewModel;
            
        }
    }
}

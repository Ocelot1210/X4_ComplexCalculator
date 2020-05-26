using System.Windows.Controls;
using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.PlanningArea
{
    /// <summary>
    /// PlanningArea.xaml の相互作用ロジック
    /// </summary>
    public partial class PlanningArea : UserControl
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlanningArea()
        {
            InitializeComponent();

            // タブ切り替え/ドッキング/ドッキング解除するたびにコンストラクタが走るためDockingManagerをここで再設定する
            // → 再設定しないと上記操作時にレイアウトが初期化される
            if (Resources["ProxyDockingManager"] is BindingProxy proxy)
            {
                proxy.Data = dockingManager;
            }
        }
    }
}

using System.Windows.Controls;
using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.WorkArea
{
    /// <summary>
    /// WorkArea.xaml の相互作用ロジック
    /// </summary>
    public partial class WorkArea : UserControl
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WorkArea()
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

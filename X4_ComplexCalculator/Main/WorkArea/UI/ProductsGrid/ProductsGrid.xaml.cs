using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid
{
    /// <summary>
    /// ProductGrid.xaml の相互作用ロジック
    /// </summary>
    public partial class ProductsGrid : UserControl
    {
        public ProductsGrid()
        {
            InitializeComponent();
        }

        private void DataGridCell_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is DataGridCell cell && !cell.IsReadOnly)
            {
                cell.IsEditing = true;
            }
        }

        private void DataGridCell_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is DataGridCell cell)
            {
                cell.IsEditing = false;
            }
        }
    }
}

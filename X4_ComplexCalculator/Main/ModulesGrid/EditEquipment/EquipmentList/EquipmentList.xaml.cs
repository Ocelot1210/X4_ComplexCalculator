using System.Windows;
using System.Windows.Controls;

namespace X4_ComplexCalculator.Main.ModulesGrid.EditEquipment.EquipmentList
{
    /// <summary>
    /// EquipmentList.xaml の相互作用ロジック
    /// </summary>
    public partial class EquipmentList : UserControl
    {
        public static readonly DependencyProperty SelectedSizeProperty =
            DependencyProperty.Register("SelectedSize", typeof(DB.X4DB.Size), typeof(EquipmentList), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedSizeChanged)));

        public DB.X4DB.Size SelectedSize
        {
            get
            {
                return (DB.X4DB.Size)GetValue(SelectedSizeProperty);
            }
            set
            {
                SetValue(SelectedSizeProperty, value);
            }
        }


        private static void OnSelectedSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            dynamic ctrl = obj as EquipmentList;
            if (ctrl != null)
            {
                ctrl.DataContext.SelectedSize = ctrl.SelectedSize;
            }
        }


        public EquipmentList()
        {
            InitializeComponent();
        }
    }
}

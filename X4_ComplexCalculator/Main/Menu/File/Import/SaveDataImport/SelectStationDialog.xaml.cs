using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace X4_ComplexCalculator.Main.Menu.File.Import.SaveDataImport
{
    /// <summary>
    /// SelectStationDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectStationDialog : Window
    {
        private SelectStationDialog(List<SaveDataStationItem> planItems)
        {
            InitializeComponent();
            DataContext = new SelectStationViewModel(planItems);
        }


        /// <summary>
        /// ダイアログ表示
        /// </summary>
        /// <param name="selectedItems">選択計画一覧</param>
        public static bool ShowDialog(List<SaveDataStationItem> stationItems)
        {
            var wnd = new SelectStationDialog(stationItems);

            wnd.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive) ?? Application.Current.MainWindow;

            return wnd.ShowDialog() == true;
        }
    }
}

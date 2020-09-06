﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace X4_ComplexCalculator.Main.Menu.File.Import.StationPlanImport
{
    /// <summary>
    /// SelectPlanWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectPlanDialog : System.Windows.Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="selectedItems">選択計画一覧</param>
        private SelectPlanDialog(List<StationPlanItem> planItems)
        {
            InitializeComponent();
            DataContext = new SelectPlanViewModel(planItems);
        }


        /// <summary>
        /// ダイアログ表示
        /// </summary>
        /// <param name="selectedItems">選択計画一覧</param>
        public static bool ShowDialog(List<StationPlanItem> planItems)
        {
            var wnd = new SelectPlanDialog(planItems);

            wnd.Owner = Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault(x => x.IsActive) ?? Application.Current.MainWindow;

            return wnd.ShowDialog() == true;
        }
    }
}

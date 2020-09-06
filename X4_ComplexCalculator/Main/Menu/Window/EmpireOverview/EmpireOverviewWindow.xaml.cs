﻿using System.Collections.ObjectModel;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main.Menu.Window.EmpireOverview
{
    /// <summary>
    /// EmpireOverviewWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EmpireOverviewWindow : System.Windows.Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="model">帝国の概要用Model</param>
        public EmpireOverviewWindow(ObservableCollection<WorkAreaViewModel> workAreas)
        {
            InitializeComponent();

            DataContext = new EmpireOverviewWindowViewModel(workAreas);
        }
    }
}

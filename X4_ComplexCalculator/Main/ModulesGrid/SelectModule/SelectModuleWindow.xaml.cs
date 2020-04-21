﻿using System.Windows;
using X4_ComplexCalculator.Common.Collection;

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
        /// <param name="prevModuleName">変更前のモジュール</param>
        public SelectModuleWindow(SmartCollection<ModulesGridItem> modules, string prevModuleName = "")
        {
            InitializeComponent();

            DataContext = new SelectModuleViewModel(modules, prevModuleName);
        }
    }
}

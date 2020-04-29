using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace X4_ComplexCalculator.Main.ModulesGrid.EditEquipment.EditPresetName
{
    /// <summary>
    /// EditPresetWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditPresetNameWindow : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="origPresetName">変更前プリセット名</param>
        public EditPresetNameWindow(string origPresetName)
        {
            InitializeComponent();

            DataContext = new EditPresetNameViewModel(origPresetName);
        }


        /// <summary>
        /// プリセット選択画面を表示
        /// </summary>
        /// <param name="origPresetName">変更前プリセット名</param>
        /// <returns></returns>
        public static string ShowDialog(string origPresetName)
        {
            var wnd = new EditPresetNameWindow(origPresetName);

            if (wnd.ShowDialog() == true)
            {
                return wnd.NewPresetNameTextBox.Text;
            }

            return "";
        }
    }
}

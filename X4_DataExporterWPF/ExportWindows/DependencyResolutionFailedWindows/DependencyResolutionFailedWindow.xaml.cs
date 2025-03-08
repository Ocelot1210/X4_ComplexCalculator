using LibX4.FileSystem;
using System.Collections.Generic;
using System.Windows;

namespace X4_DataExporterWPF.ExportWindow.DependencyResolutionFailedWindows
{
    /// <summary>
    /// DependencyResolutionFailedWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DependencyResolutionFailedWindow : Window
    {
        public DependencyResolutionFailedWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// 新しいインスタンスを作成する
        /// </summary>
        /// <param name="mods">依存関係の解決に失敗した Mod の一覧</param>
        public DependencyResolutionFailedWindow(IReadOnlyList<ModInfo> mods)
        {
            InitializeComponent();

            DataContext = new DependencyResolutionFailedModel(mods);
        }
    }
}

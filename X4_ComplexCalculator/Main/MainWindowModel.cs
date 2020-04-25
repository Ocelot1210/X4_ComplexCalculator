using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main
{
    /// <summary>
    /// メイン画面のModel
    /// </summary>
    class MainWindowModel
    {
        #region プロパティ
        /// <summary>
        /// ワークエリア一覧
        /// </summary>
        public ObservableCollection<WorkAreaViewModel> Documents = new ObservableCollection<WorkAreaViewModel>();


        /// <summary>
        /// アクティブなワークスペース
        /// </summary>
        public WorkAreaViewModel ActiveContent { set; get; }


        public bool UnsavedExists
        {
            get
            {
                return Documents.Where(x => x.HasChanged).Any();
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowModel()
        {

        }

        /// <summary>
        /// 上書き保存
        /// </summary>
        public void Save()
        {
            ActiveContent?.Save();
        }

        /// <summary>
        /// 名前を付けて保存
        /// </summary>
        public void SaveAs()
        {
            ActiveContent?.SaveAs();
        }

        /// <summary>
        /// 開く
        /// </summary>
        public void Open()
        {
            var dlg = new OpenFileDialog();

            dlg.Filter = "X4 Station calclator data (*.x4)|*.x4|All Files|*.*";
            if (dlg.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    var vm = new WorkAreaViewModel();
                    vm.LoadFile(dlg.FileName);
                    Documents.Add(vm);
                }
                catch (Exception e)
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show($"ファイルの読み込みに失敗しました。\r\n\r\n■理由：\r\n{e.Message}", "読み込み失敗", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        /// <summary>
        /// 新規作成
        /// </summary>
        public void CreateNew()
        {
            var vm = new WorkAreaViewModel();
            Documents.Add(vm);
            ActiveContent = vm;
        }


        /// <summary>
        /// DB更新
        /// </summary>
        public void UpdateDB()
        {
            var result = MessageBox.Show("DB更新画面を表示しますか？\r\n※ 画面が起動するまでしばらくお待ち下さい。", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (!DBConnection.UpdateDB())
                {
                    MessageBox.Show("DBの更新に失敗しました。\r\nDBファイルにアクセス可能か確認後、再度実行してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.WorkArea;
using X4_ComplexCalculator.Common.Dialog.SelectStringDialog;
using X4_ComplexCalculator.Main.Menu.Layout;
using X4_ComplexCalculator.Common.Collection;
using System.Collections.Generic;
using Xceed.Wpf.AvalonDock;
using X4_ComplexCalculator.Main.Menu.File.Import;
using X4_ComplexCalculator.Main.Menu.File.Export;
using Prism.Commands;

namespace X4_ComplexCalculator.Main
{
    /// <summary>
    /// メイン画面のModel
    /// </summary>
    class MainWindowModel
    {
        #region メンバ
        /// <summary>
        /// 現在のレイアウト
        /// </summary>
        private LayoutMenuItem _ActiveLayout;
        #endregion

        #region プロパティ
        /// <summary>
        /// ワークエリア一覧
        /// </summary>
        public ObservableCollection<WorkAreaViewModel> Documents = new ObservableCollection<WorkAreaViewModel>();


        /// <summary>
        /// アクティブなワークスペース
        /// </summary>
        public WorkAreaViewModel ActiveContent { set; get; }


        /// <summary>
        /// レイアウト一覧
        /// </summary>
        public ObservablePropertyChangedCollection<LayoutMenuItem> Layouts = new ObservablePropertyChangedCollection<LayoutMenuItem>();


        /// <summary>
        /// 現在のレイアウト
        /// </summary>
        private LayoutMenuItem ActiveLayout
        {
            get => _ActiveLayout;
            set
            {
                if (ActiveLayout != value)
                {
                    _ActiveLayout = value;
                    if (_ActiveLayout != null)
                    {
                        foreach (var document in Documents)
                        {
                            document.SetLayout(_ActiveLayout.LayoutID);
                        }
                    }
                }
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowModel()
        {
            Layouts.CollectionPropertyChanged += Layouts_CollectionPropertyChanged;
        }


        /// <summary>
        /// インポート実行
        /// </summary>
        /// <param name="import"></param>
        public void Import(IImport import)
        {
            var vm = new WorkAreaViewModel(ActiveLayout?.LayoutID ?? -1);

            if (vm.Import(import))
            {
                Documents.Add(vm);
            }
            else
            {
                vm.Dispose();
            }
        }

        /// <summary>
        /// エクスポート実行
        /// </summary>
        /// <param name="export"></param>
        public void Export(IExport export)
        {
            ActiveContent?.Export(export);
        }


        /// <summary>
        /// レイアウト一覧のプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Layouts_CollectionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!(sender is LayoutMenuItem menuItem))
            {
                return;
            }


            switch (e.PropertyName)
            {
                // チェック状態
                case nameof(LayoutMenuItem.IsChecked):
                    if (menuItem.IsChecked)
                    {
                        // プリセットが選択された場合、他のチェックを全部外す
                        foreach (var layout in Layouts.Where(x => x != menuItem))
                        {
                            layout.IsChecked = false;
                        }

                        ActiveLayout = menuItem;
                    }
                    else
                    {
                        ActiveLayout = null;
                    }
                    break;

                // 削除されたか
                case nameof(LayoutMenuItem.IsDeleted):
                    if (menuItem.IsDeleted)
                    {
                        Layouts.Remove(menuItem);
                        ActiveLayout = null;
                    }
                    break;

                // レイアウト上書き保存
                case nameof(LayoutMenuItem.SaveButtonClickedCommand):
                    if (ActiveContent != null)
                    {
                        ActiveContent.OverwriteSaveLayout(menuItem.LayoutID);

                        MessageBox.Show($"計画「{ActiveContent.Title}」のレイアウトを「{menuItem.LayoutName}」として上書き保存しました。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// ウィンドウがロードされた時
        /// </summary>
        public void WindowLoaded()
        {
            // DB接続開始
            DBConnection.Open();

            // レイアウト一覧読み込み
            var layouts = new List<LayoutMenuItem>();
            DBConnection.CommonDB.ExecQuery("SELECT LayoutID, LayoutName, IsChecked FROM WorkAreaLayouts", (dr, args) =>
            {
                layouts.Add(new LayoutMenuItem((long)dr["LayoutID"], (string)dr["LayoutName"], (long)dr["IsChecked"] == 1));
            });

            var checkedLayout = layouts.Where(x => x.IsChecked).FirstOrDefault();
            if (checkedLayout != null)
            {
                ActiveLayout = checkedLayout;
            }

            Layouts.AddRange(layouts);
        }

        /// <summary>
        /// レイアウト保存
        /// </summary>
        public void SaveLayout()
        {
            if (ActiveContent != null)
            {
                var (onOK, layoutName) = SelectStringDialog.ShowDialog("レイアウト名編集", "レイアウト名", "", LayoutMenuItem.IsValidLayoutName);
                if (onOK)
                {
                    var layoutID = ActiveContent.SaveLayout(layoutName);

                    Layouts.Add(new LayoutMenuItem(layoutID, layoutName, false));

                    MessageBox.Show($"計画「{ActiveContent.Title}」のレイアウトを「{layoutName}」として保存しました。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("レイアウト保存対象のタブが選択されていません。\r\n保存対象のタブを選択後、再度実行して下さい。", "確認", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
                    var vm = new WorkAreaViewModel(ActiveLayout?.LayoutID ?? -1);
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
            var vm = new WorkAreaViewModel(ActiveLayout?.LayoutID ?? -1);
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

        /// <summary>
        /// ウィンドウが閉じられる時
        /// </summary>
        /// <returns>キャンセルされたか</returns>
        public bool WindowClosing()
        {
            var canceled = false;

            // 未保存の内容が存在するか？
            if (Documents.Where(x => x.HasChanged).Any())
            {
                var result = MessageBox.Show("未保存の項目があります。保存しますか？", "確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (result)
                {
                    // 保存する場合
                    case MessageBoxResult.Yes:
                        foreach (var doc in Documents)
                        {
                            doc.Save();
                        }
                        break;

                    // 保存せずに閉じる場合
                    case MessageBoxResult.No:
                        break;

                    // キャンセルする場合
                    case MessageBoxResult.Cancel:
                        canceled = true;
                        break;
                }
            }

            // 閉じる場合、リソースを開放
            if (!canceled)
            {
                foreach (var doc in Documents)
                {
                    doc.Dispose();
                }
            }

            return canceled;
        }


        /// <summary>
        /// 作業エリアが閉じられる時
        /// </summary>
        /// <param name="vm">閉じようとしている作業エリア</param>
        /// <returns></returns>
        public bool DocumentClosing(WorkAreaViewModel vm)
        {
            var canceled = false;

            // 変更があったか？
            if (vm.HasChanged)
            {
                var result = MessageBox.Show("変更内容を保存しますか？", "確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (result)
                {
                    // 保存する場合
                    case MessageBoxResult.Yes:
                        vm.Save();
                        break;

                    // 保存せずに閉じる場合
                    case MessageBoxResult.No:
                        break;

                    // キャンセルする場合
                    case MessageBoxResult.Cancel:
                        canceled = true;
                        break;
                }
            }

            // 閉じる場合、リソースを開放
            if (!canceled)
            {
                vm.Dispose();
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => Documents.Remove(vm)), DispatcherPriority.Background);

                // 最後のタブを閉じた時にAvalonDockのActiveContentが更新されないためここでnullにする
                // → nullにしないと閉じたはずのタブを保存できてしまう
                if (Documents.Count == 1)
                {
                    ActiveContent = null;
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }

            return canceled;
        }
    }
}

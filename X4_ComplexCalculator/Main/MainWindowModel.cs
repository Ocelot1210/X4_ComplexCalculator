using Microsoft.Win32;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WPFLocalizeExtension.Engine;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Dialog.SelectStringDialog;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.Menu.File.Export;
using X4_ComplexCalculator.Main.Menu.File.Import;
using X4_ComplexCalculator.Main.Menu.Lang;
using X4_ComplexCalculator.Main.Menu.Layout;
using X4_ComplexCalculator.Main.PlanningArea;

namespace X4_ComplexCalculator.Main
{
    /// <summary>
    /// メイン画面のModel
    /// </summary>
    class MainWindowModel : BindableBase
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
        public ObservableCollection<PlanningAreaViewModel> Documents = new ObservableCollection<PlanningAreaViewModel>();


        /// <summary>
        /// アクティブなワークスペース
        /// </summary>
        public PlanningAreaViewModel ActiveContent { set; get; }


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

        /// <summary>
        /// 言語一覧
        /// </summary>
        public ObservablePropertyChangedCollection<LangMenuItem> Languages = new ObservablePropertyChangedCollection<LangMenuItem>();
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowModel()
        {
            Layouts.CollectionPropertyChanged += Layouts_CollectionPropertyChanged;

            Languages.AddRange(LocalizeDictionary.Instance.DefaultProvider.AvailableCultures.Where(x => !string.IsNullOrEmpty(x.Name)).Select(x => new LangMenuItem(x)));
            Languages.CollectionPropertyChanged += Languages_CollectionPropertyChanged;
        }

        /// <summary>
        /// 言語一覧のプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Languages_CollectionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!(sender is LangMenuItem langMenuItem))
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(LangMenuItem.IsChecked):
                    {
                        if (langMenuItem.IsChecked)
                        {
                            foreach (var lang in Languages)
                            {
                                if (!ReferenceEquals(lang, langMenuItem))
                                {
                                    lang.IsChecked = false;
                                }
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// インポート実行
        /// </summary>
        /// <param name="import"></param>
        public void Import(IImport import)
        {
            // インポート対象数を取得
            var importCnt = import.Select();

            for (var cnt = 0; cnt < importCnt; cnt++)
            {
                var vm = new PlanningAreaViewModel(ActiveLayout?.LayoutID ?? -1);

                if (vm.Import(import))
                {
                    Documents.Add(vm);
                }
                else
                {
                    vm.Dispose();
                }
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

                        Localize.ShowMessageBox("Lang:LayoutOverwritedMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, ActiveContent.Title, menuItem.LayoutName);
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
                var (onOK, layoutName) = SelectStringDialog.ShowDialog("Lang:EditLayoutName", "Lang:LayoutName", "", LayoutMenuItem.IsValidLayoutName);
                if (onOK)
                {
                    var layoutID = ActiveContent.SaveLayout(layoutName);

                    Layouts.Add(new LayoutMenuItem(layoutID, layoutName, false));

                    Localize.ShowMessageBox("Lang:LayoutSavedMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, ActiveContent.Title, layoutName);
                }
            }
            else
            {
                Localize.ShowMessageBox("Lang:TabDoesNotSelectedMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var filePath in dlg.FileNames)
                        {
                            var vm = new PlanningAreaViewModel(ActiveLayout?.LayoutID ?? -1);
                            vm.LoadFile(filePath);
                            Documents.Add(vm);
                        }
                    });
                }
                catch (Exception e)
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(() =>
                    {
                        Localize.ShowMessageBox("Lang:FaildToLoadFileMessage", "Lang:FaildToLoadFileMessageTitle", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, e.Message);
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
            var vm = new PlanningAreaViewModel(ActiveLayout?.LayoutID ?? -1);
            Documents.Add(vm);
            ActiveContent = vm;
        }


        /// <summary>
        /// DB更新
        /// </summary>
        public void UpdateDB()
        {
            var result = Localize.ShowMessageBox("Lang:DBUpdateConfirmationMessage", "Lang:Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (!DBConnection.UpdateDB())
                {
                    Localize.ShowMessageBox("Lang:DBUpdateConfirmationMessage", "Lang:Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Error);
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
                var result = Localize.ShowMessageBox("Lang:MainWindowClosingConfirmMessage", "Lang:Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

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
        public bool DocumentClosing(PlanningAreaViewModel vm)
        {
            var canceled = false;

            // 変更があったか？
            if (vm.HasChanged)
            {
                var result = Localize.ShowMessageBox("Lang:PlanClosingConfirmMessage", "Lang:Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel, vm.Title);

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

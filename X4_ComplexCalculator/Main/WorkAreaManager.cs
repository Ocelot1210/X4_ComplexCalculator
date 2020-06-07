using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Dialog.SelectStringDialog;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.Menu.Layout;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main
{
    /// <summary>
    /// 作業エリア管理用
    /// </summary>
    class WorkAreaManager
    {
        #region メンバ
        /// <summary>
        /// 現在のレイアウト
        /// </summary>
        private LayoutMenuItem? _ActiveLayout;

        /// <summary>
        /// ガーベジコレクション用ストップウォッチ
        /// </summary>
        private readonly Stopwatch _GCStopWatch = new Stopwatch();

        /// <summary>
        /// ガーベジコレクション用タイマー
        /// </summary>
        private DispatcherTimer _GCTimer;


        /// <summary>
        /// ワークエリア一覧
        /// </summary>
        public ObservableRangeCollection<WorkAreaViewModel> Documents = new ObservableRangeCollection<WorkAreaViewModel>();
        #endregion


        #region プロパティ
        /// <summary>
        /// アクティブなワークスペース
        /// </summary>
        public WorkAreaViewModel? ActiveContent { set; get; }


        /// <summary>
        /// レイアウト一覧
        /// </summary>
        public ObservablePropertyChangedCollection<LayoutMenuItem> Layouts = new ObservablePropertyChangedCollection<LayoutMenuItem>();


        /// <summary>
        /// 現在のレイアウト
        /// </summary>
        public LayoutMenuItem? ActiveLayout
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
        public WorkAreaManager()
        {
            Layouts.CollectionPropertyChanged += Layouts_CollectionPropertyChanged;

            _GCTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, new EventHandler(GarvageCollect), Application.Current.Dispatcher);
            _GCTimer.Stop();
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
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

                // ガーベジコレクション用タイマー開始
                if (!_GCTimer.IsEnabled)
                {
                    _GCTimer.Start();
                }

                // 時間計測用ストップウォッチを初期化
                _GCStopWatch.Reset();

            }

            return canceled;
        }


        /// <summary>
        /// ガーベジコレクション実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GarvageCollect(object? sender, EventArgs e)
        {
            // 最後のタブクローズから1000ミリ秒経過してからガーベジコレクションを発動する
            _GCStopWatch.Stop();
            if (1000 < _GCStopWatch.ElapsedMilliseconds)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // ガーベジコレクション無効化
                _GCTimer.Stop();
            }
            _GCStopWatch.Start();
        }
    }
}

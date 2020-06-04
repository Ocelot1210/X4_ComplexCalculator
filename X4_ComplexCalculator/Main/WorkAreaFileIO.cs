using Microsoft.Win32;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main
{
    /// <summary>
    /// WorkAreaのファイル読み書き処理用クラス
    /// </summary>
    class WorkAreaFileIO : BindableBase
    {
        #region メンバ
        /// <summary>
        /// ビジー状態か
        /// </summary>
        private bool _IsBusy;


        /// <summary>
        /// 作業エリア管理用
        /// </summary>
        private readonly WorkAreaManager _WorkAreaManager;
        #endregion


        #region プロパティ
        /// <summary>
        /// ビジー状態か
        /// </summary>
        public bool IsBusy
        {
            get => _IsBusy;
            private set => SetProperty(ref _IsBusy, value);
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="workAreaManager">作業エリア管理用</param>
        public WorkAreaFileIO(WorkAreaManager workAreaManager)
        {
            _WorkAreaManager = workAreaManager;
        }


        /// <summary>
        /// 上書き保存
        /// </summary>
        public void Save()
        {
            _WorkAreaManager.ActiveContent?.Save();
        }


        /// <summary>
        /// 名前を付けて保存
        /// </summary>
        public void SaveAs()
        {
            _WorkAreaManager.ActiveContent?.SaveAs();
        }


        /// <summary>
        /// 新規作成
        /// </summary>
        public void CreateNew()
        {
            var vm = new WorkAreaViewModel(_WorkAreaManager.ActiveLayout?.LayoutID ?? -1);
            _WorkAreaManager.Documents.Add(vm);
            _WorkAreaManager.ActiveContent = vm;
        }


        /// <summary>
        /// 開く
        /// </summary>
        public void Open()
        {
            var dlg = new OpenFileDialog();

            dlg.Filter = "X4: Complex calclator data file(*.x4)|*.x4|All Files|*.*";
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == true)
            {
                OpenFiles(dlg.FileNames);
            }
        }

        //BackgroundWorker? _BackgroundWorker;

        /// <summary>
        /// ファイルを開く
        /// </summary>
        /// <param name="pathes">開く対象のファイルパス一覧</param>
        public void OpenFiles(IEnumerable<string> pathes)
        {
            if (!pathes.Any())
            {
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                // Application.Current.Dispatcher.Invoke(() => IsBusy = true);

                //var op = Dispatcher.CurrentDispatcher.BeginInvoke(() =>
                //{
                //    AddMain(pathes);
                //});
                IsBusy = true;
                AddMain(pathes);
                //_BackgroundWorker = new BackgroundWorker();
                //_BackgroundWorker.WorkerReportsProgress = true;

                //_BackgroundWorker.DoWork += (s, evt) => AddMain(pathes);
                //_BackgroundWorker.ProgressChanged += (s, evt) => { };
                //_BackgroundWorker.RunWorkerCompleted += (s, evt) => { IsBusy = false; };

                //_BackgroundWorker.RunWorkerAsync();
            }
            catch (Exception e)
            {
                Localize.ShowMessageBox("Lang:FaildToLoadFileMessage", "Lang:FaildToLoadFileMessageTitle", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, e.Message);
            }
            finally
            {
                
            }
        }




        private void AddMain(IEnumerable<string> pathes)
        {
            //var worker = new Thread(delegate()
            //{
            //    Application.Current.Dispatcher.Invoke(() =>
            //    {
            //        var tmpList = new List<WorkAreaViewModel>();

            //        foreach (var path in pathes)
            //        {
            //            var prg = new Progress<int>();

            //            var vm = new WorkAreaViewModel(_WorkAreaManager.ActiveLayout?.LayoutID ?? -1);
            //            vm.LoadFile(path, prg);
            //            tmpList.Add(vm);
            //        }
            //        _WorkAreaManager.Documents.AddRange(tmpList);

            //        IsBusy = false;
            //        Mouse.OverrideCursor = null;
            //    });
            //});
            //worker.IsBackground = true;
            //worker.Start();


            //var tmpList = new List<WorkAreaViewModel>();
            //foreach (var path in pathes)
            //{
            //    var prg = new Progress<int>();

            //    var vm = new WorkAreaViewModel(_WorkAreaManager.ActiveLayout?.LayoutID ?? -1);
            //    vm.LoadFile(path, prg);
            //    tmpList.Add(vm);
            //}
            //_WorkAreaManager.Documents.AddRange(tmpList);
            //IsBusy = false;

            void test()
            {
                var tmpList = new List<WorkAreaViewModel>();
                foreach (var path in pathes)
                {
                    var prg = new Progress<int>();

                    var vm = new WorkAreaViewModel(_WorkAreaManager.ActiveLayout?.LayoutID ?? -1);
                    vm.LoadFile(path, prg);
                    tmpList.Add(vm);
                }
                _WorkAreaManager.Documents.AddRange(tmpList);
                IsBusy = false;
                Mouse.OverrideCursor = null;
            }

            ThreadStart start = delegate ()
            {
                var op = Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(test));

                var status = op.Status;
                while (status != DispatcherOperationStatus.Completed)
                {
                    status = op.Wait(TimeSpan.FromMilliseconds(100));
                    if (status == DispatcherOperationStatus.Aborted)
                    {
                        
                    }
                }
            };

            new Thread(start).Start();
        }
    }
}

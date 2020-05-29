using Microsoft.Win32;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
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

            dlg.Filter = "X4 Station calclator data (*.x4)|*.x4|All Files|*.*";
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    IsBusy = true;
                    var tmpList = new List<WorkAreaViewModel>();

                    foreach (var filePath in dlg.FileNames)
                    {
                        var prg = new Progress<int>();

                        var vm = new WorkAreaViewModel(_WorkAreaManager.ActiveLayout?.LayoutID ?? -1);
                        vm.LoadFile(filePath, prg);
                        tmpList.Add(vm);
                    }

                    _WorkAreaManager.Documents.AddRange(tmpList);
                }
                catch (Exception e)
                {
                    Localize.ShowMessageBox("Lang:FaildToLoadFileMessage", "Lang:FaildToLoadFileMessageTitle", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, e.Message);
                }
                finally
                {
                    IsBusy = false;
                    Mouse.OverrideCursor = null;
                }
            }
        }
    }
}

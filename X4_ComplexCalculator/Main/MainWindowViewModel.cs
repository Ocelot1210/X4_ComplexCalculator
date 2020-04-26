using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Main.WorkArea;
using Xceed.Wpf.AvalonDock;

namespace X4_ComplexCalculator.Main
{
    /// <summary>
    /// メイン画面のViewModel
    /// </summary>
    class MainWindowViewModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// メイン画面のModel
        /// </summary>
        private readonly MainWindowModel _Model;
        #endregion

        #region プロパティ
        /// <summary>
        /// Windowが閉じられる時
        /// </summary>
        public ICommand WindowClosingCommand { get; }

        /// <summary>
        /// 新規作成
        /// </summary>
        public ICommand CreateNewCommand { get; }


        /// <summary>
        /// 上書き保存
        /// </summary>
        public ICommand SaveCommand { get; }


        /// <summary>
        /// 名前を指定して保存
        /// </summary>
        public ICommand SaveAsCommand { get; }


        /// <summary>
        /// 開く
        /// </summary>
        public ICommand OpenCommand { get; }


        /// <summary>
        /// DB更新
        /// </summary>
        public ICommand UpdateDBCommand { get; }


        /// <summary>
        /// タブが閉じられる時
        /// </summary>
        public ICommand DocumentClosingCommand { get; }


        /// <summary>
        /// ワークエリア一覧
        /// </summary>
        public ObservableCollection<WorkAreaViewModel> Documents => _Model.Documents;


        /// <summary>
        /// アクティブなワークスペース
        /// </summary>
        public WorkAreaViewModel ActiveContent
        {
            set
            {
                _Model.ActiveContent = value;
            }
            get
            {
                return _Model.ActiveContent;
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            _Model                 = new MainWindowModel();
            WindowClosingCommand   = new DelegateCommand<CancelEventArgs>(WindowClosing);
            CreateNewCommand       = new DelegateCommand(CreateNew);
            SaveCommand            = new DelegateCommand(_Model.Save);
            SaveAsCommand          = new DelegateCommand(_Model.SaveAs);
            OpenCommand            = new DelegateCommand(Open);
            UpdateDBCommand        = new DelegateCommand(_Model.UpdateDB);
            DocumentClosingCommand = new DelegateCommand<DocumentClosingEventArgs>(DocumentClosing);
        }


        /// <summary>
        /// 新規作成
        /// </summary>
        private void CreateNew()
        {
            _Model.CreateNew();
            OnPropertyChanged(nameof(ActiveContent));
        }


        /// <summary>
        /// 開く
        /// </summary>
        private void Open()
        {
            _Model.Open();
            OnPropertyChanged(nameof(ActiveContent));
        }


        /// <summary>
        /// ウィンドウが閉じられる時
        /// </summary>
        private void WindowClosing(CancelEventArgs e)
        {
            e.Cancel = _Model.WindowClosing();
        }

        /// <summary>
        /// タブが閉じられる時
        /// </summary>
        /// <param name="e"></param>
        private void DocumentClosing(DocumentClosingEventArgs e)
        {
            if (e.Document.Content is WorkAreaViewModel workArea)
            {
                e.Cancel = _Model.DocumentClosing(workArea);
            }
        }
    }
}

using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using X4_ComplexCalculator.Main.Menu.File.Export;
using X4_ComplexCalculator.Main.Menu.File.Import;
using X4_ComplexCalculator.Main.Menu.File.Import.SaveDataImport;
using X4_ComplexCalculator.Main.Menu.File.Import.StationPlanImport;
using X4_ComplexCalculator.Main.Menu.Layout;
using X4_ComplexCalculator.Main.WorkArea;
using Xceed.Wpf.AvalonDock;

namespace X4_ComplexCalculator.Main
{
    /// <summary>
    /// メイン画面のViewModel
    /// </summary>
    class MainWindowViewModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// メイン画面のModel
        /// </summary>
        private readonly MainWindowModel _Model;
        #endregion

        #region プロパティ
        /// <summary>
        /// Windowがロードされた時
        /// </summary>
        public ICommand WindowLoadedCommand { get; }

        /// <summary>
        /// Windowが閉じられる時
        /// </summary>
        public ICommand WindowClosingCommand { get; }


        /// <summary>
        /// レイアウト保存
        /// </summary>
        public ICommand SaveLayout { get; }


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


        /// <summary>
        /// レイアウト一覧
        /// </summary>
        public ObservableCollection<LayoutMenuItem> Layouts => _Model.Layouts;


        /// <summary>
        /// インポート処理一覧
        /// </summary>
        public List<IImport> Imports { get; }


        /// <summary>
        /// エクスポート処理一覧
        /// </summary>
        public List<IExport> Exports { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            _Model                 = new MainWindowModel();
            WindowLoadedCommand    = new DelegateCommand(_Model.WindowLoaded);
            WindowClosingCommand   = new DelegateCommand<CancelEventArgs>(WindowClosing);
            CreateNewCommand       = new DelegateCommand(CreateNew);
            SaveLayout             = new DelegateCommand(_Model.SaveLayout);
            SaveCommand            = new DelegateCommand(_Model.Save);
            SaveAsCommand          = new DelegateCommand(_Model.SaveAs);
            OpenCommand            = new DelegateCommand(Open);
            UpdateDBCommand        = new DelegateCommand(_Model.UpdateDB);
            DocumentClosingCommand = new DelegateCommand<DocumentClosingEventArgs>(DocumentClosing);


            Imports = new List<IImport>()
            {
                new StationCalclatorImport(new DelegateCommand<IImport>(_Model.Import)),
                new StationPlanImport(new DelegateCommand<IImport>(_Model.Import)),
                //new SaveDataImport(new DelegateCommand<IImport>(_Model.Import))   // 作成中のため未リリース
            };

            Exports = new List<IExport>()
            {
                new StationCalclatorExport(new DelegateCommand<IExport>(_Model.Export))
            };
        }


        /// <summary>
        /// 新規作成
        /// </summary>
        private void CreateNew()
        {
            _Model.CreateNew();
            RaisePropertyChanged(nameof(ActiveContent));
        }


        /// <summary>
        /// 開く
        /// </summary>
        private void Open()
        {
            _Model.Open();
            RaisePropertyChanged(nameof(ActiveContent));
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

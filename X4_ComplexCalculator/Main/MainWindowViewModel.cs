using Prism.Commands;
using System.Collections.ObjectModel;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Main.WorkArea;

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
        /// 新規作成
        /// </summary>
        public DelegateCommand CreateNewCommand { get; }


        /// <summary>
        /// 上書き保存
        /// </summary>
        public DelegateCommand SaveCommand { get; }


        /// <summary>
        /// 名前を指定して保存
        /// </summary>
        public DelegateCommand SaveAsCommand { get; }


        /// <summary>
        /// 開く
        /// </summary>
        public DelegateCommand OpenCommand { get; }


        /// <summary>
        /// DB更新
        /// </summary>
        public DelegateCommand UpdateDBCommand { get; }


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
            _Model            = new MainWindowModel();
            CreateNewCommand  = new DelegateCommand(CreateNew);
            SaveCommand       = new DelegateCommand(_Model.Save);
            SaveAsCommand     = new DelegateCommand(_Model.SaveAs);
            OpenCommand       = new DelegateCommand(Open);
            UpdateDBCommand   = new DelegateCommand(_Model.UpdateDB);
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
    }
}

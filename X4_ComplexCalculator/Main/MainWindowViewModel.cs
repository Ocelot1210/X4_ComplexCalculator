using Prism.Commands;
using System.Collections.ObjectModel;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main
{
    class MainWindowViewModel
    {
        #region メンバ
        private readonly MainWindowModel _Model;
        #endregion

        #region プロパティ

        /// <summary>
        /// 新規作成
        /// </summary>
        public DelegateCommand CreateNew { get; }

        /// <summary>
        /// 上書き保存
        /// </summary>
        public DelegateCommand Save { get; }

        /// <summary>
        /// 名前を指定して保存
        /// </summary>
        public DelegateCommand SaveAs { get; }

        /// <summary>
        /// 開く
        /// </summary>
        public DelegateCommand Open { get; }


        /// <summary>
        /// DB更新
        /// </summary>
        public DelegateCommand UpdateDB { get; }


        /// <summary>
        /// ワークエリア一覧
        /// </summary>
        public ObservableCollection<WorkAreaViewModel> Documents => _Model.Documents;


        public WorkAreaViewModel ActiveContent
        {
            set
            {
                _Model.ActiveContent = value;
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            _Model    = new MainWindowModel();
            CreateNew = new DelegateCommand(_Model.CreateNew);
            Save      = new DelegateCommand(_Model.Save);
            SaveAs    = new DelegateCommand(_Model.SaveAs);
            Open      = new DelegateCommand(_Model.Open);
            UpdateDB  = new DelegateCommand(_Model.UpdateDB);
        }
    }
}

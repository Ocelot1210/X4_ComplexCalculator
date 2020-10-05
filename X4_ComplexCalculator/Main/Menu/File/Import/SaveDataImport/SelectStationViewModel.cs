using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace X4_ComplexCalculator.Main.Menu.File.Import.SaveDataImport
{
    class SelectStationViewModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// Model
        /// </summary>
        private readonly SelectStationModel _Model;


        /// <summary>
        /// ダイアログの戻り値
        /// </summary>
        private bool _DialogResult;


        /// <summary>
        /// ダイアログを閉じるか
        /// </summary>
        private bool _CloseDialogProperty;


        /// <summary>
        /// 選択された計画一覧
        /// </summary>
        private readonly List<SaveDataStationItem> _SelectedStationItems;
        #endregion


        #region プロパティ

        /// <summary>
        /// ダイアログの戻り値
        /// </summary>
        public bool DialogResult
        {
            get => _DialogResult;
            set => SetProperty(ref _DialogResult, value);
        }

        /// <summary>
        /// ダイアログを閉じるか
        /// </summary>
        public bool CloseDialogProperty
        {
            get => _CloseDialogProperty;
            set => SetProperty(ref _CloseDialogProperty, value);
        }

        public bool? IsCheckedAll
        {
            get
            {
                var @checked = Stations.Where(x => x.IsChecked).Count();

                return (@checked == 0) ? (bool?)false :
                       (@checked == Stations.Count) ? (bool?)true : null;
            }
            set
            {
                foreach (var station in Stations)
                {
                    station.IsChecked = value ?? false;
                }
            }
        }


        /// <summary>
        /// セーブデータファイルパス
        /// </summary>
        public string SaveDataFilePath => _Model.SaveDataFilePath;


        /// <summary>
        /// 計画一覧
        /// </summary>
        public ObservableCollection<SaveDataStationItem> Stations => _Model.Stations;


        /// <summary>
        /// 建造計画ファイル選択
        /// </summary>
        public ICommand SelectSaveDataFileCommand { get; }


        /// <summary>
        /// OKボタンクリック時の処理
        /// </summary>
        public ICommand OkButtonClickedCommand { get; }


        /// <summary>
        /// キャンセルボタンクリック時の処理
        /// </summary>
        public ICommand CancelButtonClickedCommand { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="stationItems"></param>
        public SelectStationViewModel(List<SaveDataStationItem> stationItems)
        {
            _Model = new SelectStationModel();
            _SelectedStationItems = stationItems;
            OkButtonClickedCommand      = new DelegateCommand(OkButtonClicked);
            CancelButtonClickedCommand  = new DelegateCommand(CancelButtonClicked);
            SelectSaveDataFileCommand   = new DelegateCommand(_Model.SelectSaveDataFile);
        }


        /// <summary>
        /// OKボタンクリック時
        /// </summary>
        private void OkButtonClicked()
        {
            _SelectedStationItems.AddRange(Stations.Where(x => x.IsChecked));
            DialogResult = true;
            CloseDialogProperty = true;
        }


        /// <summary>
        /// キャンセルボタンクリック時
        /// </summary>
        private void CancelButtonClicked()
        {
            _SelectedStationItems.Clear();
            DialogResult = false;
            CloseDialogProperty = true;
        }
    }
}

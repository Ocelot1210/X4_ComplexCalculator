using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace X4_ComplexCalculator.Main.Menu.File.Import.LoadoutImport
{
    class SelectLoadoutViewModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// Model
        /// </summary>
        private readonly SelectLoadoutModel _Model;


        /// <summary>
        /// ダイアログの戻り値
        /// </summary>
        private bool _DialogResult;


        /// <summary>
        /// ダイアログを閉じるか
        /// </summary>
        private bool _CloseDialogProperty;
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


        /// <summary>
        /// チェック/全チェック変更時
        /// </summary>
        public bool? IsCheckedAll
        {
            get
            {
                var @checked = Loadouts.Where(x => x.IsChecked).Count();

                return (@checked == 0) ? false :
                       (@checked == Loadouts.Count) ? (bool?)true : null;
            }
            set
            {
                foreach (var station in Loadouts)
                {
                    station.IsChecked = value ?? false;
                }
            }
        }


        /// <summary>
        /// セーブデータファイルパス
        /// </summary>
        public string LoadoutsFilePath => _Model.LoadoutsFilePath;


        /// <summary>
        /// 計画一覧
        /// </summary>
        public ObservableCollection<LoadoutItem> Loadouts => _Model.Loadouts;


        /// <summary>
        /// 建造計画ファイル選択
        /// </summary>
        public ICommand SelectSaveDataFileCommand { get; }


        /// <summary>
        /// インポートボタンクリック時の処理
        /// </summary>
        public ICommand ImportButtonClickedCommand { get; }


        /// <summary>
        /// 閉じるボタンクリック時の処理
        /// </summary>
        public ICommand CloseButtonClickedCommand { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SelectLoadoutViewModel()
        {
            _Model = new SelectLoadoutModel();
            ImportButtonClickedCommand = new DelegateCommand(_Model.Import);
            CloseButtonClickedCommand  = new DelegateCommand(CloseButtonClicked);
            SelectSaveDataFileCommand  = new DelegateCommand(_Model.SelectSaveDataFile);
        }


        /// <summary>
        /// 閉じるボタンクリック時
        /// </summary>
        private void CloseButtonClicked()
        {
            DialogResult = true;
            CloseDialogProperty = true;
        }
    }
}

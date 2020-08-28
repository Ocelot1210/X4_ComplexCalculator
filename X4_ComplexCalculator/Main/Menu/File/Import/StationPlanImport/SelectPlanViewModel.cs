using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace X4_ComplexCalculator.Main.Menu.File.Import.StationPlanImport
{
    class SelectPlanViewModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// Model
        /// </summary>
        private readonly SelectPlanModel _Model;


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
        private List<StationPlanItem> _SelectedPlanItems;
        #endregion


        #region プロパティ
        /// <summary>
        /// 計画一覧
        /// </summary>
        public ObservableCollection<StationPlanItem> Planes => _Model.Planes;


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
        /// 計画ファイルパス
        /// </summary>
        public string PlanFilePath => _Model.PlanFilePath;


        /// <summary>
        /// 建造計画ファイル選択
        /// </summary>
        public ICommand SelectPlanFileCommand { get; }


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
        /// <param name="selectedItems">選択計画一覧</param>
        public SelectPlanViewModel(List<StationPlanItem> planItems)
        {
            _Model = new SelectPlanModel();
            _Model.PropertyChanged += Model_PropertyChanged;
            _SelectedPlanItems = planItems;

            OkButtonClickedCommand     = new DelegateCommand(OkButtonClicked);
            CancelButtonClickedCommand = new DelegateCommand(CancelButtonClicked);
            SelectPlanFileCommand      = new DelegateCommand(_Model.SelectPlanFile);
        }


        /// <summary>
        /// Modelのプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SelectPlanModel.PlanFilePath):
                    RaisePropertyChanged(nameof(PlanFilePath));
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// OKボタンクリック時
        /// </summary>
        private void OkButtonClicked()
        {
            _SelectedPlanItems.AddRange(Planes.Where(x => x.IsChecked));
            DialogResult = true;
            CloseDialogProperty = true;
        }


        /// <summary>
        /// キャンセルボタンクリック時
        /// </summary>
        private void CancelButtonClicked()
        {
            DialogResult = false;
            CloseDialogProperty = true;
        }
    }
}

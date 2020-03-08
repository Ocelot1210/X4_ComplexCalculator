using Prism.Commands;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.ModulesGrid.EditEquipment.EquipmentList
{
    /// <summary>
    /// 装備一覧用ViewModel
    /// </summary>
    class EquipmentListViewModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 装備一覧用Model
        /// </summary>
        readonly EquipmentListModelBase Model;


        /// <summary>
        /// 装備検索文字列
        /// </summary>
        private string _SearchEquipmentName = "";


        /// <summary>
        /// 検索用フィルタを削除できるか
        /// </summary>
        private bool CanRemoveFilter = false;
        #endregion


        #region プロパティ
        /// <summary>
        /// 表示するコレクション
        /// </summary>
        private CollectionViewSource EquipmentsViewSource { get; set; }


        /// <summary>
        /// 装備一覧
        /// </summary>
        public ObservableCollection<Equipment> Equipments => (Model.SelectedSize != null)? Model.Equipments[Model.SelectedSize] : null;


        /// <summary>
        /// 装備中の装備
        /// </summary>
        public ObservableCollection<Equipment> Equipped => (Model.SelectedSize != null)? Model.Equipped[Model.SelectedSize] : null;


        /// <summary>
        /// 装備可能な個数
        /// </summary>
        public int MaxAmount => Model.MaxAmount[Model.SelectedSize];


        /// <summary>
        /// 現在装備中の個数
        /// </summary>
        public int EquippedCount => Model.Equipped[Model.SelectedSize].Count;


        /// <summary>
        /// 装備を追加可能か
        /// </summary>
        public bool CanAddEquipment => EquippedCount < MaxAmount;


        /// <summary>
        /// 装備を削除可能か
        /// </summary>
        public bool CanRemoveEquipment => 0 < EquippedCount;


        /// <summary>
        /// 装備の検索文字列
        /// </summary>
        public string SearchEquipmentName
        {
            get
            {
                return _SearchEquipmentName;
            }
            set
            {
                if (_SearchEquipmentName == value) return;
                _SearchEquipmentName = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }


        /// <summary>
        /// 追加ボタンクリック時のコマンド
        /// </summary>
        public DelegateCommand<ICollection> AddButtonClickedCommand { get; }


        /// <summary>
        /// 削除ボタンクリック時のコマンド
        /// </summary>
        public DelegateCommand<ICollection> RemoveButtonClickedCommand { get; }


        /// <summary>
        /// 選択中の装備サイズ
        /// </summary>
        public Size SelectedSize
        {
            set
            {
                Model.SelectedSize = value;
                OnPropertyChanged("MaxAmount");
                OnPropertyChanged("EquippedCount");
                OnPropertyChanged("Equipments");
                OnPropertyChanged("Equipped");
                OnPropertyChanged("CanAddEquipment");
                OnPropertyChanged("CanRemoveEquipment");
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="model"></param>
        public EquipmentListViewModel(EquipmentListModelBase model, CollectionViewSource viewSource)
        {
            Model = model;
            EquipmentsViewSource = viewSource;

            AddButtonClickedCommand = new DelegateCommand<ICollection>(AddButtonClicked);
            RemoveButtonClickedCommand = new DelegateCommand<ICollection>(DeleteButtonClicked);
        }


        /// <summary>
        /// 装備を保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SaveEquipment(object sender, EventArgs e)
        {
            Model.SaveEquipment();
        }


        /// <summary>
        /// 追加ボタンクリック時
        /// </summary>
        /// <param name="selectedEquipment">追加予定装備</param>
        void AddButtonClicked(ICollection selectedEquipments)
        {
            if (selectedEquipments != null)
            {
                Model.AddEquipments(selectedEquipments.Cast<Equipment>());
                OnPropertyChanged("CanAddEquipment");
                OnPropertyChanged("CanRemoveEquipment");
                OnPropertyChanged("MaxAmount");
                OnPropertyChanged("EquippedCount");
            }
        }


        /// <summary>
        /// 削除ボタンクリック時
        /// </summary>
        /// <param name="selectedEquipments">削除予定装備</param>
        void DeleteButtonClicked(ICollection selectedEquipments)
        {
            if (selectedEquipments != null)
            {
                Model.RemoveEquipments(selectedEquipments.Cast<Equipment>().ToArray());
                OnPropertyChanged("CanAddEquipment");
                OnPropertyChanged("CanRemoveEquipment");
                OnPropertyChanged("MaxAmount");
                OnPropertyChanged("EquippedCount");
            }
        }


        /// <summary>
        /// フィルタを適用
        /// </summary>
        private void ApplyFilter()
        {
            // 2回目以降か？
            if (CanRemoveFilter)
            {
                EquipmentsViewSource.Filter -= new FilterEventHandler(FilterEvent);
                EquipmentsViewSource.Filter += new FilterEventHandler(FilterEvent);
            }
            else
            {
                // 初回はこっち
                CanRemoveFilter = true;
                EquipmentsViewSource.Filter += new FilterEventHandler(FilterEvent);
            }
        }


        /// <summary>
        /// フィルタイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ev"></param>
        private void FilterEvent(object sender, FilterEventArgs e)
        {
            e.Accepted = e.Item is Equipment src && (SearchEquipmentName == "" || 0 <= src.Name.IndexOf(SearchEquipmentName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}

using Prism.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        readonly EquipmentListModelBase _Model;


        /// <summary>
        /// 装備検索文字列
        /// </summary>
        private string _SearchEquipmentName = "";


        /// <summary>
        /// 装備一覧表示用
        /// </summary>
        private readonly Dictionary<Size, ListCollectionView> _EquipmentsViews = new Dictionary<Size, ListCollectionView>();
        #endregion


        #region プロパティ
        /// <summary>
        /// 装備一覧表示用
        /// </summary>
        public ListCollectionView EquipmentsView => (_Model.SelectedSize != null) ? _EquipmentsViews[_Model.SelectedSize] : null;


        /// <summary>
        /// 装備中の装備
        /// </summary>
        public ObservableCollection<Equipment> Equipped => (_Model.SelectedSize != null)? _Model.Equipped[_Model.SelectedSize] : null;


        /// <summary>
        /// 装備可能な個数
        /// </summary>
        public int MaxAmount => (_Model.SelectedSize != null)? _Model.MaxAmount[_Model.SelectedSize] : 0;


        /// <summary>
        /// 現在装備中の個数
        /// </summary>
        public int EquippedCount => (_Model.SelectedSize != null) ? _Model.Equipped[_Model.SelectedSize].Count : 0;


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
                if (_SearchEquipmentName != value)
                {
                    _SearchEquipmentName = value;
                    OnPropertyChanged();
                    EquipmentsView.Refresh();
                }
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
                _Model.SelectedSize = value;
                OnPropertyChanged(nameof(MaxAmount));
                OnPropertyChanged(nameof(EquippedCount));
                OnPropertyChanged(nameof(Equipped));
                OnPropertyChanged(nameof(CanAddEquipment));
                OnPropertyChanged(nameof(CanRemoveEquipment));
                OnPropertyChanged(nameof(EquipmentsView));
            }
        }


        /// <summary>
        /// 未保存か
        /// </summary>
        public bool Unsaved { get; set; } = false;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="model"></param>
        public EquipmentListViewModel(EquipmentListModelBase model)
        {
            _Model = model;

            foreach (var pair in _Model.Equipments)
            {
                var item = (ListCollectionView)CollectionViewSource.GetDefaultView(pair.Value);
                item.Filter = Filter;
                _EquipmentsViews.Add(pair.Key, item);
            }

            AddButtonClickedCommand = new DelegateCommand<ICollection>(AddButtonClicked);
            RemoveButtonClickedCommand = new DelegateCommand<ICollection>(DeleteButtonClicked);
        }


        /// <summary>
        /// 装備を保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SaveEquipment()
        {
            _Model.SaveEquipment();
            Unsaved = false;
        }


        /// <summary>
        /// 追加ボタンクリック時
        /// </summary>
        /// <param name="selectedEquipment">追加予定装備</param>
        void AddButtonClicked(ICollection selectedEquipments)
        {
            if (selectedEquipments != null)
            {
                _Model.AddEquipments(selectedEquipments.Cast<Equipment>());
                Unsaved = true;
                OnPropertyChanged(nameof(CanAddEquipment));
                OnPropertyChanged(nameof(CanRemoveEquipment));
                OnPropertyChanged(nameof(EquippedCount));
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
                _Model.RemoveEquipments(selectedEquipments.Cast<Equipment>().ToArray());
                Unsaved = true;
                OnPropertyChanged(nameof(CanAddEquipment));
                OnPropertyChanged(nameof(CanRemoveEquipment));
                OnPropertyChanged(nameof(EquippedCount));
            }
        }


        /// <summary>
        /// フィルタイベント
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool Filter(object obj)
        {
            return obj is Equipment src && (SearchEquipmentName == "" || 0 <= src.Name.IndexOf(SearchEquipmentName, StringComparison.InvariantCultureIgnoreCase));
        }


        /// <summary>
        /// プリセット一覧変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnPresetsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _Model.OnPresetsCollectionChanged(sender, e);
            Unsaved = true;
        }
    }
}

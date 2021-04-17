using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Entity;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment.EquipmentList;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment
{
    /// <summary>
    /// 装備編集画面のViewModel
    /// </summary>
    class EditEquipmentViewModel : BindableBase, IDisposable
    {
        #region メンバ
        /// <summary>
        /// 装備編集画面のModel
        /// </summary>
        private readonly EditEquipmentModel _Model;


        /// <summary>
        /// ウィンドウの表示状態
        /// </summary>
        private bool _CloseWindow = false;


        /// <summary>
        /// ゴミ箱
        /// </summary>
        private readonly CompositeDisposable _Disposables = new();
        #endregion


        #region プロパティ
        /// <summary>
        /// 編集対象モジュール名
        /// </summary>
        public string ModuleName { get; }


        /// <summary>
        /// ウィンドウの表示状態
        /// </summary>
        public bool CloseWindowProperty
        {
            get
            {
                return _CloseWindow;
            }
            set
            {
                _CloseWindow = value;
                RaisePropertyChanged();
            }
        }


        /// <summary>
        /// ウィンドウが閉じられる時
        /// </summary>
        public ICommand WindowClosingCommand { get; }


        /// <summary>
        /// 装備サイズ一覧
        /// </summary>
        public ObservableCollection<IX4Size> EquipmentSizes => _Model.EquipmentSizes;


        /// <summary>
        /// 選択中の装備サイズ
        /// </summary>
        public ReactiveProperty<IX4Size> SelectedSize { get; }


        /// <summary>
        /// 種族一覧
        /// </summary>
        public ICollectionView FactionsView { get; }


        /// <summary>
        /// プリセット
        /// </summary>
        public ObservableCollection<PresetComboboxItem> Presets => _Model.Presets;


        /// <summary>
        /// 選択中のプリセット
        /// </summary>
        public ReactiveProperty<PresetComboboxItem?> SelectedPreset { get; }


        /// <summary>
        /// 保存ボタンクリック
        /// </summary>
        public ICommand SaveButtonClickedCommand { get; }


        /// <summary>
        /// 閉じるボタンクリック時のコマンド
        /// </summary>
        public ICommand CloseWindowCommand { get; }


        /// <summary>
        /// プリセット編集
        /// </summary>
        public ICommand EditPresetNameCommand { get; }


        /// <summary>
        /// プリセット追加
        /// </summary>
        public ICommand AddPresetCommand { get; }


        /// <summary>
        /// プリセット保存
        /// </summary>
        public ICommand OverwritePresetCommand { get; }


        /// <summary>
        /// プリセット削除
        /// </summary>
        public ICommand DeletePresetCommand { get; }


        /// <summary>
        /// タブアイテム一覧
        /// </summary>
        public ObservableCollection<EquipmentListViewModel> EquipmentListViewModels => _Model.EquipmentListViewModels;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentManager">編集対象の装備情報</param>
        public EditEquipmentViewModel(EquippableWareEquipmentManager equipmentManager)
        {
            ModuleName = equipmentManager.Ware.Name;

            // Model類
            _Model = new EditEquipmentModel(equipmentManager);


            // コマンド類
            SaveButtonClickedCommand = new DelegateCommand(SavebuttonClicked);
            CloseWindowCommand       = new DelegateCommand(CloseWindow);
            OverwritePresetCommand   = new DelegateCommand(_Model.OverwritePreset);
            EditPresetNameCommand    = new DelegateCommand(_Model.EditPresetName);
            AddPresetCommand         = new DelegateCommand(_Model.AddPreset);
            DeletePresetCommand      = new DelegateCommand(_Model.DeletePreset);
            WindowClosingCommand     = new DelegateCommand<CancelEventArgs>(WindowClosing);


            // その他初期化
            SelectedSize = _Model.SelectedSize
                .ToReactivePropertyAsSynchronized(x => x.Value)
                .AddTo(_Disposables);

            SelectedPreset = _Model.SelectedPreset
                .ToReactivePropertyAsSynchronized(x => x.Value)
                .AddTo(_Disposables);


            FactionsView = CollectionViewSource.GetDefaultView(_Model.Factions);
            FactionsView.SortDescriptions.Clear();
            FactionsView.SortDescriptions.Add(new SortDescription(nameof(FactionsListItem.RaceName), ListSortDirection.Ascending));
            FactionsView.SortDescriptions.Add(new SortDescription(nameof(FactionsListItem.FactionName), ListSortDirection.Ascending));
            FactionsView.GroupDescriptions.Clear();
            FactionsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(FactionsListItem.RaceID)));
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _Model.Dispose();
            _Disposables.Dispose();
        }


        /// <summary>
        /// ウィンドウが閉じられる時
        /// </summary>
        /// <param name="e"></param>
        public void WindowClosing(CancelEventArgs e)
        {
            // 装備が未保存の場合
            if (EquipmentListViewModels.Any(x => x.Unsaved.Value))
            {
                var result = LocalizedMessageBox.Show("Lang:EditEquipmentWindow_CloseConfirmMessage", "Lang:Common_MessageBoxTitle_Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (result)
                {
                    // 保存する場合
                    case MessageBoxResult.Yes:
                        _Model.SaveEquipment();
                        break;

                    // 保存せずに閉じる場合
                    case MessageBoxResult.No:
                        break;

                    // キャンセルする場合
                    case MessageBoxResult.Cancel:
                        CloseWindowProperty = false;
                        e.Cancel = true;
                        break;
                }
            }

            // ウィンドウを閉じる場合、チェック状態を保存
            if (!e.Cancel)
            {
                Task.Run(_Model.SaveCheckState);
                Dispose();
            }
        }


        /// <summary>
        /// 保存ボタンクリック時
        /// </summary>
        private void SavebuttonClicked()
        {
            _Model.SaveEquipment();
            CloseWindowProperty = true;
        }


        /// <summary>
        /// 閉じるボタンクリック時
        /// </summary>
        private void CloseWindow() => CloseWindowProperty = true;
    }
}

using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB;
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
        private readonly EditEquipmentModel Model;

        /// <summary>
        /// ウィンドウの表示状態
        /// </summary>
        private bool _CloseWindow = false;


        /// <summary>
        /// 選択中の装備サイズ
        /// </summary>
        private DB.X4DB.Size _SelectedSize;
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
        public ObservableCollection<DB.X4DB.Size> EquipmentSizes => Model.EquipmentSizes;


        /// <summary>
        /// 選択中の装備サイズ
        /// </summary>
        public DB.X4DB.Size SelectedSize
        {
            get
            {
                return _SelectedSize;
            }
            set
            {
                if (value.SizeID != (_SelectedSize?.SizeID ?? ""))
                {
                    _SelectedSize = value;
                    TurretsViewModel.SelectedSize = value;
                    ShieldsViewModel.SelectedSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 種族一覧
        /// </summary>
        public ObservablePropertyChangedCollection<FactionsListItem> Factions => Model.Factions;


        /// <summary>
        /// プリセット
        /// </summary>
        public ObservableCollection<PresetComboboxItem> Presets => Model.Presets;


        /// <summary>
        /// 選択中のプリセット
        /// </summary>
        public PresetComboboxItem SelectedPreset
        {
            get => Model.SelectedPreset;
            set
            {
                if (Model.SelectedPreset != value)
                {
                    Model.SelectedPreset = value;
                    TurretsViewModel.SelectedPreset = value;
                    ShieldsViewModel.SelectedPreset = value;
                }
            }
        }


        /// <summary>
        /// 保存ボタンクリック
        /// </summary>
        public ICommand SaveButtonClickedCommand { get; }


        /// <summary>
        /// 閉じるボタンクリック時のコマンド
        /// </summary>
        public ICommand CloseWindowCommand { get; }


        /// <summary>
        /// タレット用ViewModel
        /// </summary>
        public EquipmentListViewModel TurretsViewModel { get; }


        /// <summary>
        /// シールド用ViewModel
        /// </summary>
        public EquipmentListViewModel ShieldsViewModel { get; }


        /// <summary>
        /// プリセット編集
        /// </summary>
        public ICommand EditPresetCommand { get; }


        /// <summary>
        /// プリセット追加
        /// </summary>
        public ICommand AddPresetCommand { get; }


        /// <summary>
        /// プリセット保存
        /// </summary>
        public ICommand SavePresetCommand { get; }


        /// <summary>
        /// プリセット削除
        /// </summary>
        public ICommand RemovePresetCommand { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">編集対象モジュール</param>
        public EditEquipmentViewModel(Module module)
        {
            ModuleName = module.Name;

            // Model類
            Model = new EditEquipmentModel(module);
            Model.PropertyChanged += Model_PropertyChanged;
            
            // サブViewModel類
            TurretsViewModel = new EquipmentListViewModel(new TurretEquipmentListModel(module, Factions));
            Presets.CollectionChanged += TurretsViewModel.OnPresetsCollectionChanged;
            
            ShieldsViewModel = new EquipmentListViewModel(new ShieldEquipmentListModel(module, Factions));
            Presets.CollectionChanged += ShieldsViewModel.OnPresetsCollectionChanged;

            // コマンド類
            SaveButtonClickedCommand = new DelegateCommand(SavebuttonClicked);
            CloseWindowCommand       = new DelegateCommand(CloseWindow);
            SavePresetCommand        = new DelegateCommand(SavePreset);
            EditPresetCommand        = new DelegateCommand(Model.EditPreset);
            AddPresetCommand         = new DelegateCommand(Model.AddPreset);
            RemovePresetCommand      = new DelegateCommand(Model.RemovePreset);
            WindowClosingCommand     = new DelegateCommand<CancelEventArgs>(WindowClosing);

            // その他初期化
            SelectedSize = EquipmentSizes.FirstOrDefault();
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            Presets.CollectionChanged -= TurretsViewModel.OnPresetsCollectionChanged;
            Presets.CollectionChanged -= ShieldsViewModel.OnPresetsCollectionChanged;

            TurretsViewModel.Dispose();
            ShieldsViewModel.Dispose();
        }


        /// <summary>
        /// Modelのプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Model.SelectedPreset):
                    RaisePropertyChanged(nameof(SelectedPreset));
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// ウィンドウが閉じられる時
        /// </summary>
        /// <param name="e"></param>
        public void WindowClosing(CancelEventArgs e)
        {
            // 装備が未保存の場合
            if (TurretsViewModel.Unsaved || ShieldsViewModel.Unsaved)
            {
                var result = MessageBox.Show("変更内容を保存しますか？", "確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (result)
                {
                    // 保存する場合
                    case MessageBoxResult.Yes:
                        TurretsViewModel.SaveEquipment();
                        ShieldsViewModel.SaveEquipment();
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
                Model.SaveCheckState();
                Dispose();
            }
        }


        /// <summary>
        /// 保存ボタンクリック時
        /// </summary>
        private void SavebuttonClicked()
        {
            TurretsViewModel.SaveEquipment();
            ShieldsViewModel.SaveEquipment();
            CloseWindowProperty = true;
        }


        /// <summary>
        /// 閉じるボタンクリック時
        /// </summary>
        private void CloseWindow()
        {
            CloseWindowProperty = true;
        }


        /// <summary>
        /// プリセット保存
        /// </summary>
        private void SavePreset()
        {
            TurretsViewModel.SavePreset();
            ShieldsViewModel.SavePreset();
        }
    }
}

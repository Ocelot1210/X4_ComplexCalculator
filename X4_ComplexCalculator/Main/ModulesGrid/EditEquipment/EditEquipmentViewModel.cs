using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.ModulesGrid.EditEquipment
{
    /// <summary>
    /// 装備編集画面のViewModel
    /// </summary>
    class EditEquipmentViewModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 装備編集画面のModel
        /// </summary>
        private readonly EditEquipmentModel Model;


        /// <summary>
        /// 装備保存イベント
        /// </summary>
        public event EventHandler OnSaveEquipment;
        #endregion


        #region プロパティ
        /// <summary>
        /// 編集対象モジュール名
        /// </summary>
        public string ModuleName { get; }


        /// <summary>
        /// 装備サイズ一覧
        /// </summary>
        public ObservableCollection<DB.X4DB.Size> EquipmentSizes => Model.EquipmentSizes;


        /// <summary>
        /// 種族一覧
        /// </summary>
        public MemberChangeDetectCollection<FactionsListItem> Factions => Model.Factions;


        /// <summary>
        /// プリセット
        /// </summary>
        public ObservableCollection<PresetComboboxItem> Presets => Model.Presets;


        /// <summary>
        /// 選択中のプリセット
        /// </summary>
        public PresetComboboxItem SelectedPreset
        {
            get
            {
                return Model.SelectedPreset;
            }
            set
            {
                Model.SelectedPreset = value;
            }
        }


        /// <summary>
        /// 保存ボタンクリック
        /// </summary>
        public DelegateCommand<Window> SaveButtonClickedCommand { get; }


        /// <summary>
        /// 閉じるボタンクリック時のコマンド
        /// </summary>
        public DelegateCommand<Window> CloseButtonClickedCommand { get; }


        /// <summary>
        /// プリセット保存
        /// </summary>
        public DelegateCommand SavePresetCommand { get; }


        /// <summary>
        /// プリセット追加
        /// </summary>
        public DelegateCommand AddPresetCommand { get; }


        /// <summary>
        /// プリセット削除
        /// </summary>
        public DelegateCommand RemovePresetCommand { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">編集対象モジュール</param>
        public EditEquipmentViewModel(Module module)
        {
            ModuleName = module.Name;

            Model = new EditEquipmentModel(module);
            Model.PropertyChanged += Model_PropertyChanged;
            SaveButtonClickedCommand = new DelegateCommand<Window>(SavebuttonClicked);
            CloseButtonClickedCommand = new DelegateCommand<Window>(CloseButtonClicked);
            SavePresetCommand = new DelegateCommand(Model.SavePreset);
            AddPresetCommand = new DelegateCommand(Model.AddPreset);
            RemovePresetCommand = new DelegateCommand(Model.RemovePreset);
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
                case nameof(SelectedPreset):
                    OnPropertyChanged(nameof(SelectedPreset));
                    break;

                default:
                    break;
            }
        }



        /// <summary>
        /// ウィンドウが閉じられる時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnWindowClosing(object sender, EventArgs e)
        {
            Model.SaveCheckState();
        }


        /// <summary>
        /// 保存ボタンクリック時
        /// </summary>
        /// <param name="window"></param>
        private void SavebuttonClicked(Window window)
        {
            OnSaveEquipment?.Invoke(this, null);
            window.Close();
        }


        /// <summary>
        /// 閉じるボタンクリック時
        /// </summary>
        /// <param name="window">親ウィンドウ</param>
        private void CloseButtonClicked(Window window)
        {
            window.Close();
        }
    }
}

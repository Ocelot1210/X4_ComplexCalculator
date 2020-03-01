using Prism.Commands;
using System.Windows;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.ModulesGrid.EditEquipment;

namespace X4_ComplexCalculator.Main.ModulesGrid
{
    /// <summary>
    /// ModuleクラスをDataGrid表示用クラス
    /// </summary>
    public class ModulesGridItem : INotifyPropertyChangedBace
    {
        #region "メンバ変数"
        /// <summary>
        /// モジュール数
        /// </summary>
        private int _ModuleCount = 1;
        #endregion


        #region "プロパティ

        /// <summary>
        /// モジュール
        /// </summary>
        public Module Module { get; private set; }


        /// <summary>
        /// モジュールの装備編集
        /// </summary>
        public DelegateCommand EditEquipmentCommand { get; }


        /// <summary>
        /// 選択されているか
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// モジュール数
        /// </summary>
        public int ModuleCount
        {
            get
            {
                return _ModuleCount;
            }
            set
            {
                if (0 <= value)
                {
                    _ModuleCount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 装備中のタレットの個数
        /// </summary>
        public int TurretsCount => Module.Equipment.Turret.AllEquipmentsCount;


        /// <summary>
        /// 装備中のシールドの個数
        /// </summary>
        public int ShieldsCount => Module.Equipment.Shield.AllEquipmentsCount;


        /// <summary>
        /// 編集ボタンを表示すべきか
        /// </summary>
        public Visibility EditEquipmentButtonVisiblity => (Module.Equipment.CanEquipped) ? Visibility.Visible : Visibility.Hidden;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">モジュールID</param>
        public ModulesGridItem(string id)
        {
            Module = new Module(id);

            EditEquipmentCommand = new DelegateCommand(EditEquipment);
        }


        /// <summary>
        /// 装備を編集
        /// </summary>
        private void EditEquipment()
        {
            var window = new EditEquipmentWindow(Module);

            window.ShowDialog();

            OnPropertyChanged("Module");
            OnPropertyChanged("TurretsCount");
            OnPropertyChanged("ShieldsCount");
        }
    }
}

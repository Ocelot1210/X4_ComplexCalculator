using Prism.Commands;
using System.Windows;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.ModulesGrid.EditEquipment;
using System.Linq;
using System.Collections.Generic;
using System;

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
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="modulesGridItem">コピー対象</param>
        public ModulesGridItem(ModulesGridItem modulesGridItem)
        {
            Module = new Module(modulesGridItem.Module);
            ModuleCount = modulesGridItem.ModuleCount;
            EditEquipmentCommand = new DelegateCommand(EditEquipment);
        }


        /// <summary>
        /// 装備を編集
        /// </summary>
        private void EditEquipment()
        {
            var window = new EditEquipmentWindow(Module);

            // 変更前
            var turretsOld = Module.Equipment.Turret.AllEquipments.Select(x => x.EquipmentID).OrderBy(x => x).ToArray();
            var shieldsOld = Module.Equipment.Shield.AllEquipments.Select(x => x.EquipmentID).OrderBy(x => x).ToArray();

            window.ShowDialog();

            // 変更後
            var turretsNew = Module.Equipment.Turret.AllEquipments.Select(x => x.EquipmentID).OrderBy(x => x).ToArray();
            var shieldsNew = Module.Equipment.Shield.AllEquipments.Select(x => x.EquipmentID).OrderBy(x => x).ToArray();

            bool moduleChanged = false;

            // 変更があった場合のみ通知
            if (!turretsOld.SequenceEqual(turretsNew))
            {
                OnPropertyChanged("TurretsCount");
                moduleChanged = true;
            }

            if (!shieldsOld.SequenceEqual(shieldsNew))
            {
                OnPropertyChanged("ShieldsCount");
                moduleChanged = true;
            }

            if (moduleChanged)
            {
                OnPropertyChanged("Module");
            }
        }


        public override bool Equals(object obj)
        {
            return obj is ModulesGridItem item &&
                   EqualityComparer<Module>.Default.Equals(Module, item.Module) &&
                   IsSelected == item.IsSelected &&
                   ModuleCount == item.ModuleCount &&
                   TurretsCount == item.TurretsCount &&
                   ShieldsCount == item.ShieldsCount;
                    
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Module, IsSelected, ModuleCount, TurretsCount, ShieldsCount);
        }
    }
}

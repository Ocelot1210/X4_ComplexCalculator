using Prism.Commands;
using System.Windows;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.ModulesGrid.EditEquipment;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text;

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
        private long _ModuleCount = 1;

        /// <summary>
        /// 選択された建造方式
        /// </summary>
        private ModuleProduction _SelectedMethod;
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
        public long ModuleCount
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
        /// タレットのツールチップ文字列
        /// </summary>
        public string TurretsToolTip
        {
            get
            {
                return MakeEquipmentToolTipString(Module.Equipment.Turret);
            }
        }


        /// <summary>
        /// 装備中のシールドの個数
        /// </summary>
        public int ShieldsCount => Module.Equipment.Shield.AllEquipmentsCount;

        /// <summary>
        /// シールドのツールチップ文字列
        /// </summary>
        public string ShieldsToolTip
        {
            get
            {
                return MakeEquipmentToolTipString(Module.Equipment.Shield);
            }
        }


        /// <summary>
        /// 編集ボタンを表示すべきか
        /// </summary>
        public Visibility EditEquipmentButtonVisiblity => (Module.Equipment.CanEquipped) ? Visibility.Visible : Visibility.Hidden;


        /// <summary>
        /// 建造方式を表示すべきか
        /// </summary>
        public Visibility SelectedMethodVisiblity => (2 <= Module.ModuleProductions.Count()) ? Visibility.Visible : Visibility.Hidden;


        /// <summary>
        /// 選択中の建造方式
        /// </summary>
        public ModuleProduction SelectedMethod
        {
            get
            {
                return _SelectedMethod;
            }
            set
            {
                _SelectedMethod = value;
                OnPropertyChanged();
                OnPropertyChanged("SelectedMethod.Method");
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="moduleCount">モジュール数</param>
        public ModulesGridItem(string moduleID, long moduleCount = 1)
        {
            Module = new Module(moduleID);
            ModuleCount = moduleCount;
            EditEquipmentCommand = new DelegateCommand(EditEquipment);
            _SelectedMethod = Module.ModuleProductions[0];
        }

        ~ModulesGridItem()
        {

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
            _SelectedMethod = modulesGridItem.SelectedMethod;
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
                OnPropertyChanged("TurretsToolTip");
                moduleChanged = true;
            }

            if (!shieldsOld.SequenceEqual(shieldsNew))
            {
                OnPropertyChanged("ShieldsCount");
                OnPropertyChanged("ShieldsToolTip");
                moduleChanged = true;
            }

            if (moduleChanged)
            {
                OnPropertyChanged("Module");
            }
        }

        /// <summary>
        /// 装備のツールチップ文字列を作成
        /// </summary>
        /// <returns></returns>
        private string MakeEquipmentToolTipString(ModuleEquipmentManager equipmentManager)
        {
            var sb = new StringBuilder();

            foreach (var size in equipmentManager.Sizes)
            {
                var cnt = 1;

                foreach (var eq in equipmentManager.GetEquipment(size))
                {
                    if (cnt == 1)
                    {
                        if (sb.Length != 0)
                        {
                            sb.AppendLine();
                        }
                        sb.AppendLine($"【{size.Name}】");
                    }
                    sb.AppendLine($"{cnt++:D2} ： {eq.Name}");
                }
            }

            if (sb.Length == 0)
            {
                sb.Append("何も装備していません");
            }

            return sb.ToString();
        }
    }
}

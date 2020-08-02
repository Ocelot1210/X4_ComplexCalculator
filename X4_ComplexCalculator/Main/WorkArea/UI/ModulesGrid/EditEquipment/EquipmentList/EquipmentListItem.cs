using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment.EquipmentList
{
    /// <summary>
    /// 兵装編集画面の装備品一覧1レコード分
    /// </summary>
    class EquipmentListItem : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 選択されているか
        /// </summary>
        private bool _IsSelected;
        #endregion


        #region プロパティ
        /// <summary>
        /// 装備品
        /// </summary>
        public Equipment Equipment { get; }


        /// <summary>
        /// 選択されているか
        /// </summary>
        public bool IsSelected
        {
            get => _IsSelected;
            set => SetProperty(ref _IsSelected, value);
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipment">装備品</param>
        public EquipmentListItem(Equipment equipment)
        {
            Equipment = equipment;
            _IsSelected = false;
        }
    }
}

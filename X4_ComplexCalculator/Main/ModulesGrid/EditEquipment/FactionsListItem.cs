﻿using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.ModulesGrid.EditEquipment
{
    /// <summary>
    /// 派閥リストの1レコード分
    /// </summary>
    class FactionsListItem : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// チェックされたか
        /// </summary>
        private bool _checked = true;
        #endregion

        #region プロパティ
        /// <summary>
        /// 種族
        /// </summary>
        public Faction Faction { get; }


        /// <summary>
        /// チェック状態
        /// </summary>
        public bool Checked
        {
            get
            {
                return _checked;
            }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">派閥ID</param>
        /// <param name="checkState">チェック状態</param>
        public FactionsListItem(string id, bool checkState)
        {
            Faction = new Faction(id);
            Checked = checkState;

        }
    }
}

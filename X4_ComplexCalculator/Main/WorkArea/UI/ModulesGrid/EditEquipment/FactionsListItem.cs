using System;
using Prism.Mvvm;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment
{
    /// <summary>
    /// 派閥リストの1レコード分
    /// </summary>
    class FactionsListItem : BindableBase
    {
        #region メンバ
        /// <summary>
        /// チェックされたか
        /// </summary>
        private bool _IsChecked = true;
        #endregion

        #region プロパティ
        /// <summary>
        /// 派閥
        /// </summary>
        public Faction Faction { get; }


        /// <summary>
        /// 種族ID
        /// </summary>
        public string RaceID => Faction.Race.RaceID;


        /// <summary>
        /// 種族名
        /// </summary>
        public string RaceName => Faction.Race.Name;


        /// <summary>
        /// 派閥名
        /// </summary>
        public string FactionName => Faction.Name;


        /// <summary>
        /// チェック状態
        /// </summary>
        public bool IsChecked
        {
            get => _IsChecked;
            set => SetProperty(ref _IsChecked, value);
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">派閥ID</param>
        /// <param name="isChecked">チェック状態</param>
        public FactionsListItem(string id, bool isChecked)
        {
            var faction = Faction.Get(id);
            Faction = faction ?? throw new ArgumentException("${id} is invalid factionID.");
            IsChecked = isChecked;
        }
    }
}

using Prism.Mvvm;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

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
        public IFaction Faction { get; }


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
        /// <param name="faction">派閥</param>
        /// <param name="isChecked">チェック状態</param>
        public FactionsListItem(IFaction faction, bool isChecked)
        {
            Faction = faction;
            IsChecked = isChecked;
        }
    }
}

using Prism.Mvvm;
using X4_ComplexCalculator.Common;
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
        /// 種族
        /// </summary>
        public Faction Faction { get; }


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
            Faction = Faction.Get(id);
            IsChecked = isChecked;
        }
    }
}

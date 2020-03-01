using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.ModulesGrid.EditEquipment
{
    public class SizeComboBoxItem
    {
        #region "メンバ変数"
        /// <summary>
        /// サイズ
        /// </summary>
        private Size _size;
        #endregion


        #region "プロパティ"
        /// <summary>
        /// ID
        /// </summary>
        public string ID
        {
            get
            {
                return _size.SizeID;
            }

        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get
            {
                return _size.Name;
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">サイズID</param>
        public SizeComboBoxItem(string id)
        {
            _size = new Size(id);
        }
    }
}

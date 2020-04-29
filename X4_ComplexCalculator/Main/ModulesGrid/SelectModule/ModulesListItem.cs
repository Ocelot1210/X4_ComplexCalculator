using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.ModulesGrid.SelectModule
{
    public class ModulesListItem : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// チェックされたか
        /// </summary>
        private bool _Checked;
        #endregion


        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ID { get; private set; }


        /// <summary>
        /// 表示名称
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// チェック状態
        /// </summary>
        public bool Checked
        {
            get => _Checked;
            set => SetProperty(ref _Checked, value);
        }
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="name">名称</param>
        /// <param name="checkState">チェック状態</param>
        public ModulesListItem(string id, string name, bool checkState)
        {
            ID = id;
            Name = name;
            Checked = checkState;
        }
    }
}

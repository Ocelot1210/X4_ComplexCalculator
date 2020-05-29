using Prism.Mvvm;
using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.SelectModule
{
    public class ModulesListItem : BindableBase
    {
        #region メンバ
        /// <summary>
        /// チェックされたか
        /// </summary>
        private bool _IsChecked;
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
        public bool IsChecked
        {
            get => _IsChecked;
            set => SetProperty(ref _IsChecked, value);
        }
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="name">名称</param>
        /// <param name="isChecked">チェック状態</param>
        public ModulesListItem(string id, string name, bool isChecked)
        {
            ID = id;
            Name = name;
            IsChecked = isChecked;
        }
    }
}

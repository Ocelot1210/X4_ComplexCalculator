using Prism.Mvvm;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main.Menu.View.EmpireOverview
{
    public class WorkAreaItem : BindableBase
    {
        /// <summary>
        /// 集計対象か
        /// </summary>
        private bool _isChecked;


        /// <summary>
        /// 集計対象か
        /// </summary>
        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        /// <summary>
        /// 計画
        /// </summary>
        public WorkAreaViewModel WorkArea { get; }


        /// <summary>
        /// 計画名
        /// </summary>
        public string Title => WorkArea.Title;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="workArea">計画</param>
        /// <param name="isChecked">集計対象か</param>
        public WorkAreaItem(WorkAreaViewModel workArea, bool isChecked)
        {
            WorkArea = workArea;
            _isChecked = isChecked;
        }
    }
}

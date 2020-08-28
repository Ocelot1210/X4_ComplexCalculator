using System.Xml.Linq;
using Prism.Mvvm;

namespace X4_ComplexCalculator.Main.Menu.File.Import.StationPlanImport
{
    /// <summary>
    /// ステーション計画1レコード分
    /// </summary>
    public class StationPlanItem : BindableBase
    {
        #region メンバ
        /// <summary>
        /// チェックされたか
        /// </summary>
        public bool _IsChecked;
        #endregion


        #region プロパティ
        /// <summary>
        /// 計画ID
        /// </summary>
        public string PlanID { get; }


        /// <summary>
        /// 計画名
        /// </summary>
        public string PlanName { get; }


        /// <summary>
        /// チェックされたか
        /// </summary>
        public bool IsChecked
        {
            get => _IsChecked;
            set => SetProperty(ref _IsChecked, value);
        }


        /// <summary>
        /// 計画
        /// </summary>
        public XElement Plan { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="plan">計画xml</param>
        public StationPlanItem(XElement plan)
        {
            PlanID = plan.Attribute("id").Value;
            PlanName = plan.Attribute("name").Value;

            Plan = plan;
        }
    }
}

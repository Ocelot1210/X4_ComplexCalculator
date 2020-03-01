using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Main.ModulesGrid;
using X4_ComplexCalculator.Main.ProductsGrid;
using X4_ComplexCalculator.Main.ResourcesGrid;
using X4_ComplexCalculator.Main.StorageGrid;

namespace X4_ComplexCalculator.Main.StationSummary
{
    class StationSummaryViewModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 労働力用Model
        /// </summary>
        private readonly StationSummaryWorkForceModel WorkForceModel;
        #endregion

        #region 労働力関連
        /// <summary>
        /// 労働力のタイトル
        /// </summary>
        public string WorkforceTitle => $"労働力[{WorkForceModel.WorkForce}/{WorkForceModel.NeedWorkforce}]";

        /// <summary>
        /// 労働力情報詳細
        /// </summary>
        public ObservableCollection<WorkForceDetailsItem> WorkforceDetails => WorkForceModel.WorkForceDetails;
        #endregion


        /// <summary>
        /// 1時間あたりの利益
        /// </summary>
        public string ProfitPerHourTitle => "1時間あたりの利益[1000cr]";


        /// <summary>
        /// 建造費用
        /// </summary>
        public string ConstructionCostsTitle => "建造費用[1000 cr]";


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">モジュール一覧のModel</param>
        public StationSummaryViewModel(ModulesGridModel moduleGridModel)
        {
            WorkForceModel = new StationSummaryWorkForceModel(moduleGridModel);
            WorkForceModel.PropertyChanged += WorkForceModel_PropertyChanged;
        }

        private void WorkForceModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("WorkforceTitle");
        }
    }
}

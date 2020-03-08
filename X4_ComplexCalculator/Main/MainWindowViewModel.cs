using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using X4_ComplexCalculator.Main.ModulesGrid;
using X4_ComplexCalculator.Main.ProductsGrid;
using X4_ComplexCalculator.Main.ResourcesGrid;
using X4_ComplexCalculator.Main.StoragesGrid;

namespace X4_ComplexCalculator.Main
{
    class MainWindowViewModel
    {
        #region メンバ
        private MainWindowModel Model;
        #endregion

        #region プロパティ
        /// <summary>
        /// 上書き保存
        /// </summary>
        public DelegateCommand Save { get; }

        /// <summary>
        /// 名前を指定して保存
        /// </summary>
        public DelegateCommand SaveAs { get; }

        /// <summary>
        /// 開く
        /// </summary>
        public DelegateCommand Open { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modulesGridModel">モジュール一覧用Model</param>
        /// <param name="productsGridModel">製品一覧用Model</param>
        /// <param name="storagesGridModel">建造リソース用Model</param>
        public MainWindowViewModel(ModulesGridModel modulesGridModel, ProductsGridModel productsGridModel, ResourcesGridModel resources)
        {
            Model  = new MainWindowModel(modulesGridModel.Modules, productsGridModel.Products, resources.Resources);
            Save   = new DelegateCommand(Model.Save);
            SaveAs = new DelegateCommand(Model.SaveAs);
            Open   = new DelegateCommand(Model.Open);
        }
    }
}

using Prism.Commands;
using System;
using System.IO;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Main.ModulesGrid;
using X4_ComplexCalculator.Main.ProductsGrid;
using X4_ComplexCalculator.Main.ResourcesGrid;
using X4_ComplexCalculator.Main.StationSummary;
using X4_ComplexCalculator.Main.StoragesGrid;

namespace X4_ComplexCalculator.Main.WorkArea
{
    class WorkAreaViewModel : INotifyPropertyChangedBace
    {
        #region メンバ
        private readonly WorkAreaModel _Model;
        #endregion

        #region プロパティ
        /// <summary>
        /// モジュール一覧
        /// </summary>
        public ModulesGridViewModel Modules { get; }


        /// <summary>
        /// 製品一覧
        /// </summary>
        public ProductsGridViewModel Products { get; }

        
        /// <summary>
        /// 建造リソース一覧
        /// </summary>
        public ResourcesGridViewModel Resources { get; }

        
        /// <summary>
        /// ストレージ一覧
        /// </summary>
        public StoragesGridViewModel Storages { get; }


        /// <summary>
        /// 概要
        /// </summary>
        public StationSummaryViewModel Summary { get; }


        /// <summary>
        /// タブのタイトル文字列
        /// </summary>
        public string Title => string.IsNullOrEmpty(_Model.SaveFilePath) ? "no title*" : Path.GetFileNameWithoutExtension(_Model.SaveFilePath);
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WorkAreaViewModel()
        {
            var moduleModel = new ModulesGridModel();
            var productsModel = new ProductsGridModel(moduleModel.Modules);
            var resourcesModel = new ResourcesGridModel(moduleModel.Modules);

            Summary = new StationSummaryViewModel(moduleModel.Modules, productsModel.Products, resourcesModel.Resources);
            Modules = new ModulesGridViewModel(moduleModel);
            Products = new ProductsGridViewModel(productsModel);
            Resources = new ResourcesGridViewModel(resourcesModel);
            Storages = new StoragesGridViewModel(moduleModel.Modules);

            _Model = new WorkAreaModel(moduleModel, productsModel, resourcesModel);
        }

        /// <summary>
        /// 上書き保存
        /// </summary>
        public void Save()
        {
            _Model.Save();
            OnPropertyChanged("Title");
        }

        /// <summary>
        /// 名前を付けて保存
        /// </summary>
        public void SaveAs()
        {
            _Model.SaveAs();
            OnPropertyChanged("Title");
        }


        /// <summary>
        /// ファイル読み込み
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void Load(string path)
        {
            _Model.Load(path);
            OnPropertyChanged("Title");
        }
    }
}

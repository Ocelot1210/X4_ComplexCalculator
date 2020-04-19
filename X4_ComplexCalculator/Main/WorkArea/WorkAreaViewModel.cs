using Prism.Commands;
using System.IO;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Main.ModulesGrid;
using X4_ComplexCalculator.Main.ProductsGrid;
using X4_ComplexCalculator.Main.ResourcesGrid;
using X4_ComplexCalculator.Main.StationSummary;
using X4_ComplexCalculator.Main.StoragesGrid;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace X4_ComplexCalculator.Main.WorkArea
{
    /// <summary>
    /// 作業エリア用ViewModel
    /// </summary>
    class WorkAreaViewModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// モデル
        /// </summary>
        private readonly WorkAreaModel _Model;

        /// <summary>
        /// レイアウト保持用
        /// </summary>
        private byte[] _Layout;
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


        /// <summary>
        /// ロード時
        /// </summary>
        public DelegateCommand<DockingManager> OnLoadedCommand { get; }


        /// <summary>
        /// アンロード時
        /// </summary>
        public DelegateCommand<DockingManager> OnUnloadedCommand { get; }
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
            OnLoadedCommand = new DelegateCommand<DockingManager>(OnLoaded);
            OnUnloadedCommand = new DelegateCommand<DockingManager>(OnUnloaded);
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
        public void LoadFile(string path)
        {
            _Model.Load(path);
            OnPropertyChanged("Title");
        }


        /// <summary>
        /// ロード時
        /// </summary>
        /// <param name="dockingManager"></param>
        private void OnLoaded(DockingManager dockingManager)
        {
            if (_Layout != null)
            {
                var serializer = new XmlLayoutSerializer(dockingManager);

                using var ms = new MemoryStream(_Layout);
                serializer.Deserialize(ms);
            }
        }


        /// <summary>
        /// アンロード時
        /// </summary>
        /// <param name="dockingManager"></param>
        private void OnUnloaded(DockingManager dockingManager)
        {
            var serializer = new XmlLayoutSerializer(dockingManager);
            using var ms = new MemoryStream();
            serializer.Serialize(ms);
            _Layout = ms.ToArray();
        }
    }
}

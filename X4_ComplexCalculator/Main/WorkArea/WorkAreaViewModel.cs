using Prism.Commands;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
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
    class WorkAreaViewModel : INotifyPropertyChangedBace, IDisposable
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
        public string Title
        {
            get
            {
                if (string.IsNullOrEmpty(_Model.SaveFilePath))
                {
                    return "no title*";
                }

                var ret = Path.GetFileNameWithoutExtension(_Model.SaveFilePath);

                return (HasChanged) ? $"{ret}*" : ret;
            }
        }


        /// <summary>
        /// ロード時
        /// </summary>
        public ICommand OnLoadedCommand { get; }


        /// <summary>
        /// アンロード時
        /// </summary>
        public ICommand OnUnloadedCommand { get; }


        /// <summary>
        /// モジュールの内容に変更があったか
        /// </summary>
        public bool HasChanged => _Model.HasChanged;
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

            WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.AddHandler(_Model, "PropertyChanged", Model_PropertyChanged);
        }


        /// <summary>
        /// 上書き保存
        /// </summary>
        public void Save()
        {
            _Model.Save();
        }


        /// <summary>
        /// 名前を付けて保存
        /// </summary>
        public void SaveAs()
        {
            _Model.SaveAs();
        }


        /// <summary>
        /// ファイル読み込み
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void LoadFile(string path)
        {
            _Model.Load(path);
        }


        /// <summary>
        /// ロード時
        /// </summary>
        /// <param name="dockingManager"></param>
        private void OnLoaded(DockingManager dockingManager)
        {
            // 前回レイアウトがあれば、レイアウト復元
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
            // レイアウト保存
            var serializer = new XmlLayoutSerializer(dockingManager);
            using var ms = new MemoryStream();
            serializer.Serialize(ms);
            _Layout = ms.ToArray();
        }


        /// <summary>
        /// Modelのプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_Model.HasChanged):
                    OnPropertyChanged(nameof(HasChanged));
                    OnPropertyChanged(nameof(Title));
                    break;

                case nameof(_Model.SaveFilePath):
                    OnPropertyChanged(nameof(Title));
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _Model.Dispose();
            Summary.Dispose();
            Modules.Dispose();
            Products.Dispose();
            Resources.Dispose();
            Storages.Dispose();
        }
    }
}

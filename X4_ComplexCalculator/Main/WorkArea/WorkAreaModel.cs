using Prism.Mvvm;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.SaveDataReader;
using X4_ComplexCalculator.Main.WorkArea.SaveDataWriter;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;

namespace X4_ComplexCalculator.Main.WorkArea
{
    /// <summary>
    /// 作業エリア用Model
    /// </summary>
    class WorkAreaModel : BindableBase, IDisposable, IWorkArea
    {
        #region メンバ
        /// <summary>
        /// タイトル文字列
        /// </summary>
        private string _Title = "";


        /// <summary>
        /// モジュール一覧
        /// </summary>
        private readonly ModulesGridModel _Modules;


        /// <summary>
        /// 製品一覧
        /// </summary>
        private readonly ProductsGridModel _Products;


        /// <summary>
        /// 建造リソース一覧
        /// </summary>
        private readonly ResourcesGridModel _Resources;


        /// <summary>
        /// 保管庫割当
        /// </summary>
        private readonly StorageAssignModel _StorageAssign;


        /// <summary>
        /// 変更されたか
        /// </summary>
        private bool _HasChanged;


        /// <summary>
        /// 保存ファイル書き込み用
        /// </summary>
        private readonly ISaveDataWriter _SaveDataWriter;
        #endregion


        #region プロパティ
        /// <summary>
        /// タイトル文字列
        /// </summary>
        public string Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }


        /// <summary>
        /// モジュール一覧
        /// </summary>
        public ObservableRangeCollection<ModulesGridItem> Modules => _Modules.Modules;


        /// <summary>
        /// 製品一覧
        /// </summary>
        public ObservableRangeCollection<ProductsGridItem> Products => _Products.Products;


        /// <summary>
        /// 建造リソース一覧
        /// </summary>
        public ObservableRangeCollection<ResourcesGridItem> Resources => _Resources.Resources;


        /// <summary>
        /// 保管庫割当情報
        /// </summary>
        public ObservableRangeCollection<StorageAssignGridItem> StorageAssign => _StorageAssign.StorageAssignGridItems;


        /// <summary>
        /// 変更されたか
        /// </summary>
        public bool HasChanged
        {
            get
            {
                return _HasChanged;
            }
            set
            {
                if (value != _HasChanged)
                {
                    _HasChanged = value;
                    RaisePropertyChanged();

                    if (value)
                    {
                        // 変更検知イベントを購読解除
                        _Modules.Modules.CollectionChanged -= OnModulesChanged;
                        _Modules.Modules.CollectionPropertyChanged -= OnPropertyChanged;
                        _Products.Products.CollectionPropertyChanged -= OnPropertyChanged;
                        _Resources.Resources.CollectionPropertyChanged -= OnPropertyChanged;
                        _StorageAssign.StorageAssignGridItems.CollectionPropertyChanged -= OnPropertyChanged;
                    }
                    else
                    {
                        // 変更検知イベントを購読
                        _Modules.Modules.CollectionChanged += OnModulesChanged;
                        _Modules.Modules.CollectionPropertyChanged += OnPropertyChanged;
                        _Products.Products.CollectionPropertyChanged += OnPropertyChanged;
                        _Resources.Resources.CollectionPropertyChanged += OnPropertyChanged;
                        _StorageAssign.StorageAssignGridItems.CollectionPropertyChanged += OnPropertyChanged;
                    }
                }
            }
        }


        /// <summary>
        /// 保存先ファイルパス
        /// </summary>
        public string SaveFilePath => _SaveDataWriter.SaveFilePath;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modules">モジュール一覧</param>
        /// <param name="products">製品一覧</param>
        /// <param name="resources">建造リソース一覧</param>
        /// <param name="storageAssignModel">保管庫割当</param>
        public WorkAreaModel(ModulesGridModel modules, ProductsGridModel products, ResourcesGridModel resources, StorageAssignModel storageAssignModel)
        {
            _Modules = modules;
            _Products = products;
            _Resources = resources;
            _StorageAssign = storageAssignModel;

            HasChanged = true;
            _SaveDataWriter = new SQLiteSaveDataWriter();
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            // 変更検知イベントを購読解除
            _Modules.Modules.CollectionChanged -= OnModulesChanged;
            _Modules.Modules.CollectionPropertyChanged -= OnPropertyChanged;
            _Products.Products.CollectionPropertyChanged -= OnPropertyChanged;
            _Resources.Resources.CollectionPropertyChanged -= OnPropertyChanged;
            _StorageAssign.StorageAssignGridItems.CollectionPropertyChanged -= OnPropertyChanged;
        }


        /// <summary>
        /// モジュール数に変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModulesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HasChanged = true;
        }

        /// <summary>
        /// コレクションのプロパティに変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string[] names =
            {
                nameof(ModulesGridItem.ModuleCount),
                nameof(ProductsGridItem.Price),
                nameof(ResourcesGridItem.Price),
                nameof(StorageAssignGridItem.AllocCount)
            };

            if (0 < Array.IndexOf(names, e.PropertyName))
            {
                HasChanged = true;
            }
        }


        /// <summary>
        /// 上書き保存
        /// </summary>
        public void Save()
        {
            if (_SaveDataWriter.Save(this))
            {
                HasChanged = false;
            }
        }


        /// <summary>
        /// 名前を指定して保存
        /// </summary>
        public void SaveAs()
        {
            if (_SaveDataWriter.SaveAs(this))
            {
                HasChanged = false;
            }
        }


        /// <summary>
        /// ファイル読み込み
        /// </summary>
        /// <param name="path">読み込み対象ファイルパス</param>
        /// <param name="progress">進捗</param>
        public bool Load(string path, IProgress<int> progress)
        {
            var reader = SaveDataReaderFactory.CreateSaveDataReader(path, this);
            var ret = false;

            // 読み込み成功？
            if (reader.Load(progress))
            {
                _SaveDataWriter.SaveFilePath = path;
                HasChanged = false;
                ret = true;
            }

            return ret;
        }
    }
}

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Prism.Mvvm;
using X4_ComplexCalculator.Main.WorkArea.SaveDataReader;
using X4_ComplexCalculator.Main.WorkArea.SaveDataWriter;
using X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

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
        /// 変更されたか
        /// </summary>
        private bool _HasChanged;


        /// <summary>
        /// 保存ファイル書き込み用
        /// </summary>
        private readonly ISaveDataWriter _SaveDataWriter = new SQLiteSaveDataWriter();
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
        /// 計算機で使用するステーション用データ
        /// </summary>
        public StationData StationData { get; } = new StationData();


        /// <summary>
        /// 変更されたか
        /// </summary>
        public bool HasChanged
        {
            get => _HasChanged;
            set
            {
                if (SetProperty(ref _HasChanged, value))
                {
                    if (value)
                    {
                        // 変更検知イベントを購読解除
                        StationData.Modules.CollectionChanged -= OnModulesChanged;
                        StationData.Modules.CollectionPropertyChanged -= OnPropertyChanged;
                        StationData.Products.CollectionPropertyChanged -= OnPropertyChanged;
                        StationData.BuildResources.CollectionPropertyChanged -= OnPropertyChanged;
                        StationData.StorageAssignInfo.CollectionPropertyChanged -= OnPropertyChanged;
                        StationData.Settings.PropertyChanged -= OnPropertyChanged;
                        StationData.Settings.Workforce.PropertyChanged -= OnPropertyChanged;
                    }
                    else
                    {
                        // 変更検知イベントを購読
                        StationData.Modules.CollectionChanged += OnModulesChanged;
                        StationData.Modules.CollectionPropertyChanged += OnPropertyChanged;
                        StationData.Products.CollectionPropertyChanged += OnPropertyChanged;
                        StationData.BuildResources.CollectionPropertyChanged += OnPropertyChanged;
                        StationData.StorageAssignInfo.CollectionPropertyChanged += OnPropertyChanged;
                        StationData.Settings.PropertyChanged += OnPropertyChanged;
                        StationData.Settings.Workforce.PropertyChanged += OnPropertyChanged;
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
        public WorkAreaModel()
        {
            HasChanged = true;
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            // 変更検知イベントを購読解除
            StationData.Modules.CollectionChanged -= OnModulesChanged;
            StationData.Modules.CollectionPropertyChanged -= OnPropertyChanged;
            StationData.Products.CollectionPropertyChanged -= OnPropertyChanged;
            StationData.BuildResources.CollectionPropertyChanged -= OnPropertyChanged;
            StationData.StorageAssignInfo.CollectionPropertyChanged -= OnPropertyChanged;
            StationData.Settings.PropertyChanged -= OnPropertyChanged;
            StationData.Settings.Workforce.PropertyChanged -= OnPropertyChanged;
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
                nameof(BuildResourcesGridItem.Price),
                nameof(StorageAssignGridItem.AllocCount),
                nameof(StationSettings.IsHeadquarters),
                nameof(StationSettings.Sunlight),
                nameof(WorkforceManager.Actual),
                nameof(WorkforceManager.AlwaysMaximum)
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

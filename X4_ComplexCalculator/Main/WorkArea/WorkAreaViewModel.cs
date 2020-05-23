using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.Menu.File.Export;
using X4_ComplexCalculator.Main.Menu.File.Import;
using X4_ComplexCalculator.Main.WorkArea.UI.Menu.View;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.StationSummary;
using X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;
using X4_ComplexCalculator.Main.WorkArea.UI.StoragesGrid;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using WPFLocalizeExtension.Engine;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections.Generic;
using System.Text;

namespace X4_ComplexCalculator.Main.WorkArea
{
    /// <summary>
    /// 作業エリア用ViewModel
    /// </summary>
    class WorkAreaViewModel : BindableBase, IDisposable
    {
        #region メンバ
        /// <summary>
        /// モデル
        /// </summary>
        private readonly WorkAreaModel _Model;


        /// <summary>
        /// レイアウトID
        /// </summary>
        private long _LayoutID;


        /// <summary>
        /// 現在のドッキングマネージャー
        /// </summary>
        private DockingManager _CurrentDockingManager;


        /// <summary>
        /// レイアウト保持用
        /// </summary>
        private byte[] _Layout;
        #endregion


        #region プロパティ
        /// <summary>
        /// 表示/非表示用メニューアイテム一覧
        /// </summary>
        public ObservableRangeCollection<VisiblityMenuItem> VisiblityMenuItems { get; } = new ObservableRangeCollection<VisiblityMenuItem>();


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
        /// 保管庫一覧
        /// </summary>
        public StoragesGridViewModel Storages { get; }


        /// <summary>
        /// 保管庫割当情報
        /// </summary>
        public StorageAssignViewModel StorageAssign { get; }


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
                if (string.IsNullOrEmpty(_Model.Title))
                {
                    return "no title*";
                }

                var ret = _Model.Title;

                return (HasChanged) ? $"{ret}*" : ret;
            }
        }

        /// <summary>
        /// 現在のドッキングマネージャー
        /// </summary>
        public DockingManager CurrentDockingManager
        {
            set
            {
                if (SetProperty(ref _CurrentDockingManager, value))
                {
                    RestoreLayout();
                }
            }
            private get => _CurrentDockingManager;
        }


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
        /// <param name="layoutID">レイアウトID</param>
        /// <remarks>
        /// レイアウトIDが負の場合、レイアウトは指定されていない事にする
        /// </remarks>
        public WorkAreaViewModel(long layoutID = -1)
        {
            _LayoutID = layoutID;

            var moduleModel     = new ModulesGridModel();
            var productsModel   = new ProductsGridModel(moduleModel.Modules);
            var resourcesModel  = new ResourcesGridModel(moduleModel.Modules);
            var storagesModel   = new StoragesGridModel(moduleModel.Modules);

            Summary       = new StationSummaryViewModel(moduleModel.Modules, productsModel.Products, resourcesModel.Resources);
            Modules       = new ModulesGridViewModel(moduleModel);
            Products      = new ProductsGridViewModel(productsModel);
            Resources     = new ResourcesGridViewModel(resourcesModel);
            Storages      = new StoragesGridViewModel(storagesModel);
            var storageAssignModel = new StorageAssignModel(productsModel.Products, storagesModel.Storages);
            StorageAssign = new StorageAssignViewModel(storageAssignModel);

            Modules.AutoAddModuleCommand = Products.AutoAddModuleCommand;
            _Model              = new WorkAreaModel(moduleModel, productsModel, resourcesModel, storageAssignModel);
            OnUnloadedCommand   = new DelegateCommand(OnUnloaded);

            _Model.PropertyChanged += Model_PropertyChanged;
            LocalizeDictionary.Instance.PropertyChanged += Instance_PropertyChanged;
        }


        /// <summary>
        /// 言語変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocalizeDictionary.Instance.Culture))
            {
                var serializer = new XmlLayoutSerializer(_CurrentDockingManager);

                using var ms = new MemoryStream(GetCurrentLayout(), false);
                using var ms2 = new MemoryStream(SetTitle(ms), false);
                serializer.Deserialize(ms2);

                // 表示メニューを初期化
                VisiblityMenuItems.Reset(_CurrentDockingManager.Layout.Descendents().OfType<LayoutAnchorable>().Select(x => new VisiblityMenuItem(x)));
            }
        }

        /// <summary>
        /// インポート実行
        /// </summary>
        /// <param name="import"></param>
        public bool Import(IImport import)
        {
            return import.Import(_Model);
        }


        /// <summary>
        /// エクスポート実行
        /// </summary>
        /// <param name="import"></param>
        public bool Export(IExport export)
        {
            return export.Export(_Model);
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
        /// レイアウト保存
        /// </summary>
        /// <param name="layoutName">レイアウト名</param>
        /// <returns>レイアウトID</returns>
        public long SaveLayout(string layoutName)
        {
            var id = 0L;

            var query = @$"
SELECT
    ifnull(MIN( LayoutID + 1 ), 0) AS LayoutID
FROM
    WorkAreaLayouts
WHERE
    ( LayoutID + 1 ) NOT IN ( SELECT LayoutID FROM WorkAreaLayouts)";

            DBConnection.CommonDB.ExecQuery(query, (dr, args) =>
            {
                id = (long)dr["LayoutID"];
            });

            
            var param = new SQLiteCommandParameters(3);
            param.Add("layoutID",   System.Data.DbType.Int32,   id);
            param.Add("layoutName", System.Data.DbType.String,  layoutName);
            param.Add("layout",     System.Data.DbType.Binary, GetCurrentLayout());

            DBConnection.CommonDB.ExecQuery("INSERT INTO WorkAreaLayouts(LayoutID, LayoutName, Layout) VALUES(:layoutID, :layoutName, :layout)", param);

            return id;
        }

        /// <summary>
        /// レイアウトを上書き保存
        /// </summary>
        /// <param name="layoutID">レイアウトID</param>
        public void OverwriteSaveLayout(long layoutID)
        {
            var param = new SQLiteCommandParameters(2);
            param.Add("layout", System.Data.DbType.Binary, GetCurrentLayout());
            param.Add("LayoutID", System.Data.DbType.Int32, layoutID);
            DBConnection.CommonDB.ExecQuery($"UPDATE WorkAreaLayouts SET Layout = :layout WHERE LayoutID = :layoutID", param);
        }


        /// <summary>
        /// レイアウトを設定
        /// </summary>
        /// <param name="layoutID"></param>
        public void SetLayout(long layoutID)
        {
            DBConnection.CommonDB.ExecQuery($"SELECT Layout FROM WorkAreaLayouts WHERE LayoutID = {layoutID}", (dr, args) =>
            {
                _Layout = (byte[])dr["Layout"];
            });

            if (_Layout != null)
            {
                var serializer = new XmlLayoutSerializer(_CurrentDockingManager);

                using var ms = new MemoryStream(_Layout, false);
                using var ms2 = new MemoryStream(SetTitle(ms), false);
                serializer.Deserialize(ms2);
            }

            // 表示メニューを初期化
            VisiblityMenuItems.Reset(_CurrentDockingManager.Layout.Descendents().OfType<LayoutAnchorable>().Select(x => new VisiblityMenuItem(x)));
        }


        /// <summary>
        /// 現在のレイアウトを取得
        /// </summary>
        /// <returns></returns>
        private byte[] GetCurrentLayout()
        {
            // レイアウト保存
            var serializer = new XmlLayoutSerializer(_CurrentDockingManager);
            using var ms = new MemoryStream();
            serializer.Serialize(ms);
            ms.Position = 0;

            return ms.ToArray();
        }


        /// <summary>
        /// レイアウトを復元する
        /// </summary>
        private void RestoreLayout()
        {
            // レイアウトIDが指定されていればレイアウト設定
            if (0 <= _LayoutID)
            {
                SetLayout(_LayoutID);
                // 1回ロードしたので次回以降ロードしないようにする
                _LayoutID = -1;
                return;
            }

            // 前回レイアウトがあれば、レイアウト復元
            if (_Layout != null)
            {
                var serializer = new XmlLayoutSerializer(_CurrentDockingManager);

                using var ms = new MemoryStream(_Layout, false);
                using var ms2 = new MemoryStream(SetTitle(ms), false);
                serializer.Deserialize(ms2);
            }

            // 表示メニューを初期化
            VisiblityMenuItems.Reset(_CurrentDockingManager.Layout.Descendents().OfType<LayoutAnchorable>().Select(x => new VisiblityMenuItem(x)));
        }


        /// <summary>
        /// アンロード時
        /// </summary>
        private void OnUnloaded()
        {
            _Layout = GetCurrentLayout();
        }


        /// <summary>
        /// タイトルを再設定する
        /// </summary>
        /// <param name="stream"></param>
        private byte[] SetTitle(Stream stream)
        {
            var xml = XDocument.Load(stream);

            // コンテンツIDと言語IDのペア
            var titleDict = new Dictionary<string, string>()
            {
                { "Modules",    "Lang:ModuleList" },        // モジュール一覧
                { "Products",   "Lang:Products" },          // 製品一覧
                { "Resources",  "Lang:BuildResources" },    // 建造リソース一覧
                { "Storages",   "Lang:Storages" },          // 保管庫一覧
                { "StorageSim", "Lang:StorageAssign" },     // 保管庫割当
                { "Summary",    "Lang:Summary" },           // 概要
            };

            // タイトル再設定
            foreach (var elm in xml.XPathSelectElements("LayoutRoot//LayoutAnchorable"))
            {
                if (titleDict.TryGetValue(elm.Attribute("ContentId").Value, out var langID))
                {
                    elm.Attribute("Title").Value = (string)LocalizeDictionary.Instance.GetLocalizedObject(langID, null, null);
                }
            }

            var sb = new StringBuilder();
            using var writer = new StringWriter(sb);
            xml.Save(writer);

            return Encoding.Unicode.GetBytes(sb.ToString());
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
                    RaisePropertyChanged(nameof(HasChanged));
                    RaisePropertyChanged(nameof(Title));
                    break;

                case nameof(_Model.Title):
                    RaisePropertyChanged(nameof(Title));
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
            _Model.PropertyChanged -= Model_PropertyChanged;
            LocalizeDictionary.Instance.PropertyChanged -= Instance_PropertyChanged;
            _Model.Dispose();
            Summary.Dispose();
            Modules.Dispose();
            Products.Dispose();
            Resources.Dispose();
            Storages.Dispose();
        }
    }
}

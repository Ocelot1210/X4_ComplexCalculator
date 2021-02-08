using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml.Linq;
using System.Xml.XPath;
using WPFLocalizeExtension.Engine;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.Menu.File.Export;
using X4_ComplexCalculator.Main.Menu.File.Import;
using X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.Menu.Tab;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.StationSummary;
using X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;
using X4_ComplexCalculator.Main.WorkArea.UI.StoragesGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

namespace X4_ComplexCalculator.Main.WorkArea
{
    /// <summary>
    /// 作業エリア用ViewModel
    /// </summary>
    public class WorkAreaViewModel : BindableBase, IDisposable
    {
        #region メンバ
        /// <summary>
        /// モデル
        /// </summary>
        private readonly WorkAreaModel _Model = new();


        /// <summary>
        /// レイアウトID
        /// </summary>
        private long _LayoutID;


        /// <summary>
        /// 現在のドッキングマネージャー
        /// </summary>
        private DockingManager? _CurrentDockingManager;


        /// <summary>
        /// レイアウト保持用
        /// </summary>
        private byte[]? _Layout;
        #endregion


        #region プロパティ
        /// <summary>
        /// 表示/非表示用メニューアイテム一覧
        /// </summary>
        public ObservableRangeCollection<VisiblityMenuItem> VisiblityMenuItems { get; } = new();


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
        public BuildResourcesGridViewModel Resources { get; }


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
        /// 設定
        /// </summary>
        public IStationSettings Settings => _Model.StationData.Settings;


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
        /// アンロード時
        /// </summary>
        public ICommand OnLoadedCommand { get; }


        /// <summary>
        /// モジュールの内容に変更があったか
        /// </summary>
        public bool HasChanged => _Model.HasChanged;


        /// <summary>
        /// 保存先ファイルパス
        /// </summary>
        public string SaveFilePath => _Model.SaveFilePath;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layoutID">レイアウトID</param>
        /// <remarks>
        /// レイアウトIDが負の場合、レイアウトは指定されていない事にする
        /// </remarks>
        public WorkAreaViewModel(long layoutID)
        {
            _LayoutID = layoutID;

            Summary       = new StationSummaryViewModel(_Model.StationData);
            Modules       = new ModulesGridViewModel(_Model.StationData);
            Products      = new ProductsGridViewModel(_Model.StationData);
            Resources     = new BuildResourcesGridViewModel(_Model.StationData);
            Storages      = new StoragesGridViewModel(_Model.StationData);
            StorageAssign = new StorageAssignViewModel(_Model.StationData);

            Modules.AutoAddModuleCommand = Products.AutoAddModuleCommand;
            OnLoadedCommand     = new DelegateCommand<DockingManager>(OnLoaded);

            _Model.PropertyChanged += Model_PropertyChanged;
            LocalizeDictionary.Instance.PropertyChanged += Instance_PropertyChanged;
        }


        /// <summary>
        /// 言語変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocalizeDictionary.Instance.Culture) && _CurrentDockingManager is not null)
            {
                var serializer = new XmlLayoutSerializer(_CurrentDockingManager);

                var layout = GetCurrentLayout();
                if (layout is not null)
                {
                    using var ms = new MemoryStream(layout, false);
                    using var ms2 = new MemoryStream(SetTitle(ms), false);
                    serializer.Deserialize(ms2);

                    // 表示メニューを初期化
                    VisiblityMenuItems.Reset(_CurrentDockingManager.Layout.Descendents().OfType<LayoutAnchorable>().Select(x => new VisiblityMenuItem(x)));
                }
            }
        }

        /// <summary>
        /// インポート実行
        /// </summary>
        /// <param name="import"></param>
        public bool Import(IImport import) => import.Import(_Model);


        /// <summary>
        /// エクスポート実行
        /// </summary>
        /// <param name="export"></param>
        public bool Export(IExport export) => export.Export(_Model);


        /// <summary>
        /// 上書き保存
        /// </summary>
        public void Save() => _Model.Save();


        /// <summary>
        /// 名前を付けて保存
        /// </summary>
        public void SaveAs() => _Model.SaveAs();


        /// <summary>
        /// ファイル読み込み
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public bool LoadFile(string path, IProgress<int> progress) => _Model.Load(path, progress);


        /// <summary>
        /// レイアウト保存
        /// </summary>
        /// <param name="layoutName">レイアウト名</param>
        /// <returns>レイアウトID</returns>
        public long SaveLayout(string layoutName)
        {
            var id = SettingDatabase.Instance.GetLastLayoutID();

            var param = new
            {
                LayoutID = id,
                LayoutName = layoutName,
                Layout = GetCurrentLayout() ?? throw new InvalidOperationException(),
            };

            const string sql = "INSERT INTO WorkAreaLayouts(LayoutID, LayoutName, Layout) VALUES(:LayoutID, :LayoutName, :Layout)";
            SettingDatabase.Instance.Execute(sql, param);

            return id;
        }

        /// <summary>
        /// レイアウトを上書き保存
        /// </summary>
        /// <param name="layoutID">レイアウトID</param>
        public void OverwriteSaveLayout(long layoutID)
        {
            var param = new
            {
                LayoutID = layoutID,
                Layout = GetCurrentLayout() ?? throw new InvalidOperationException(),
            };

            const string sql = "UPDATE WorkAreaLayouts SET Layout = :Layout WHERE LayoutID = :LayoutID";
            SettingDatabase.Instance.Execute(sql, param);
        }


        /// <summary>
        /// レイアウトを設定
        /// </summary>
        /// <param name="layoutID"></param>
        public void SetLayout(long layoutID)
        {
            _LayoutID = layoutID;
            _Layout = SettingDatabase.Instance.QuerySingle<byte[]>("SELECT Layout FROM WorkAreaLayouts WHERE LayoutID = :layoutID", new { layoutID });

            if (_Layout is not null && _CurrentDockingManager is not null)
            {
                var serializer = new XmlLayoutSerializer(_CurrentDockingManager);

                using var ms = new MemoryStream(_Layout, false);
                using var ms2 = new MemoryStream(SetTitle(ms), false);
                serializer.Deserialize(ms2);
            }

            // 表示メニューを初期化
            if (_CurrentDockingManager is not null)
            {
                VisiblityMenuItems.Reset(_CurrentDockingManager.Layout.Descendents().OfType<LayoutAnchorable>().Select(x => new VisiblityMenuItem(x)));
            }
        }


        /// <summary>
        /// 現在のレイアウトを取得
        /// </summary>
        /// <returns>現在のレイアウトを表す UTF-8 XML</returns>
        private byte[]? GetCurrentLayout()
        {
            if (_CurrentDockingManager is null)
            {
                return _Layout;
            }

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

            if (_CurrentDockingManager is null)
            {
                return;
            }

            // 前回レイアウトがあれば、レイアウト復元
            if (_Layout is not null)
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
        /// ロード時
        /// </summary>
        private void OnLoaded(DockingManager dockingManager)
        {
            _CurrentDockingManager = dockingManager;
            RestoreLayout();
        }


        /// <summary>
        /// タイトルを再設定する
        /// </summary>
        /// <param name="stream"></param>
        /// <remarks>
        /// ここでタイトルを再設定しないと言語を切り替えてもそれぞれのタブのタイトルが変更されない
        /// </remarks>
        private byte[] SetTitle(Stream stream)
        {
            var xml = XDocument.Load(stream);

            // コンテンツIDと言語IDのペア
            var titleDict = new Dictionary<string, string>()
            {
                { "Modules",        "Lang:ModuleList" },        // モジュール一覧
                { "Products",       "Lang:Products" },          // 製品一覧
                { "BuildResources", "Lang:BuildResources" },    // 建造リソース一覧
                { "Storages",       "Lang:Storages" },          // 保管庫一覧
                { "StorageAssign",  "Lang:StorageAssign" },     // 保管庫割当
                { "Summary",        "Lang:Summary" },           // 概要
                { "Settings",       "Lang:Settings" },          // 設定
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
        private void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
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

using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
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

namespace X4_ComplexCalculator.Main.WorkArea;

/// <summary>
/// 作業エリア用ViewModel
/// </summary>
public sealed class WorkAreaViewModel : BindableBase, IDisposable
{
    #region メンバ
    /// <summary>
    /// モデル
    /// </summary>
    private readonly WorkAreaModel _model = new();


    /// <summary>
    /// レイアウトID
    /// </summary>
    private long _layoutID;


    /// <summary>
    /// 現在のドッキングマネージャー
    /// </summary>
    private DockingManager? _currentDockingManager;


    /// <summary>
    /// レイアウト保持用
    /// </summary>
    private byte[]? _layout;
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
    public IStationSettings Settings => _model.StationData.Settings;


    /// <summary>
    /// タブのタイトル文字列
    /// </summary>
    public string Title
    {
        get
        {
            if (string.IsNullOrEmpty(_model.Title))
            {
                return "no title*";
            }

            var ret = _model.Title;

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
    public bool HasChanged => _model.HasChanged;


    /// <summary>
    /// 保存先ファイルパス
    /// </summary>
    public string SaveFilePath => _model.SaveFilePath;
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
        _layoutID = layoutID;

        Summary       = new StationSummaryViewModel(_model.StationData);
        Modules       = new ModulesGridViewModel(_model.StationData);
        Products      = new ProductsGridViewModel(_model.StationData);
        Resources     = new BuildResourcesGridViewModel(_model.StationData);
        Storages      = new StoragesGridViewModel(_model.StationData);
        StorageAssign = new StorageAssignViewModel(_model.StationData);

        Modules.AutoAddModuleCommand = Products.AutoAddModuleCommand;
        OnLoadedCommand     = new DelegateCommand<DockingManager>(OnLoaded);

        _model.PropertyChanged += Model_PropertyChanged;
        LocalizeDictionary.Instance.PropertyChanged += Instance_PropertyChanged;
    }


    /// <summary>
    /// 言語変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Instance_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LocalizeDictionary.Instance.Culture) && _currentDockingManager is not null)
        {
            var layout = GetCurrentLayout();
            if (layout is not null)
            {
                using var ms = new MemoryStream(layout, false);

                var serializer = new XmlLayoutSerializer(_currentDockingManager);
                serializer.LayoutSerializationCallback += LayoutSerializeCallback;
                serializer.Deserialize(ms);
                
                // 表示メニューを初期化
                VisiblityMenuItems.Reset(_currentDockingManager.Layout.Descendents().OfType<LayoutAnchorable>().Select(x => new VisiblityMenuItem(x)));
            }
        }
    }

    /// <summary>
    /// インポート実行
    /// </summary>
    /// <param name="import"></param>
    public bool Import(IImport import) => import.Import(_model);


    /// <summary>
    /// エクスポート実行
    /// </summary>
    /// <param name="export"></param>
    public bool Export(IExport export) => export.Export(_model);


    /// <summary>
    /// 上書き保存
    /// </summary>
    public void Save() => _model.Save();


    /// <summary>
    /// 名前を付けて保存
    /// </summary>
    public void SaveAs() => _model.SaveAs();


    /// <summary>
    /// ファイル読み込み
    /// </summary>
    /// <param name="path">ファイルパス</param>
    public bool LoadFile(string path, IProgress<int> progress) => _model.Load(path, progress);


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

        const string SQL = "INSERT INTO WorkAreaLayouts(LayoutID, LayoutName, Layout) VALUES(:LayoutID, :LayoutName, :Layout)";
        SettingDatabase.Instance.Execute(SQL, param);

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

        const string SQL = "UPDATE WorkAreaLayouts SET Layout = :Layout WHERE LayoutID = :LayoutID";
        SettingDatabase.Instance.Execute(SQL, param);
    }


    /// <summary>
    /// レイアウトを設定
    /// </summary>
    /// <param name="layoutID"></param>
    public void SetLayout(long layoutID)
    {
        _layoutID = layoutID;
        _layout = SettingDatabase.Instance.QuerySingle<byte[]>("SELECT Layout FROM WorkAreaLayouts WHERE LayoutID = :layoutID", new { layoutID });

        if (_layout is not null && _currentDockingManager is not null)
        {
            var serializer = new XmlLayoutSerializer(_currentDockingManager);
            serializer.LayoutSerializationCallback += LayoutSerializeCallback;

            using var ms = new MemoryStream(_layout, false);
            serializer.Deserialize(ms);
        }

        // 表示メニューを初期化
        if (_currentDockingManager is not null)
        {
            VisiblityMenuItems.Reset(_currentDockingManager.Layout.Descendents().OfType<LayoutAnchorable>().Select(x => new VisiblityMenuItem(x)));
        }
    }


    /// <summary>
    /// 現在のレイアウトを取得
    /// </summary>
    /// <returns>現在のレイアウトを表す UTF-8 XML</returns>
    private byte[]? GetCurrentLayout()
    {
        if (_currentDockingManager is null)
        {
            return _layout;
        }

        // レイアウト保存
        var serializer = new XmlLayoutSerializer(_currentDockingManager);
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
        if (0 <= _layoutID)
        {
            SetLayout(_layoutID);
            // 1回ロードしたので次回以降ロードしないようにする
            _layoutID = -1;
            return;
        }

        if (_currentDockingManager is null)
        {
            return;
        }

        // 前回レイアウトがあれば、レイアウト復元
        if (_layout is not null)
        {
            var serializer = new XmlLayoutSerializer(_currentDockingManager);
            serializer.LayoutSerializationCallback += LayoutSerializeCallback;
            using var ms = new MemoryStream(_layout, false);
            serializer.Deserialize(ms);
        }

        // 表示メニューを初期化
        VisiblityMenuItems.Reset(_currentDockingManager.Layout.Descendents().OfType<LayoutAnchorable>().Select(x => new VisiblityMenuItem(x)));
    }


    /// <summary>
    /// ロード時
    /// </summary>
    private void OnLoaded(DockingManager dockingManager)
    {
        _currentDockingManager = dockingManager;
        RestoreLayout();
    }


    /// <summary>
    /// レイアウトをシリアライズする際のコールバック (多言語化対応用)
    /// </summary>
    private static void LayoutSerializeCallback(object? sender, LayoutSerializationCallbackEventArgs e)
    {
        var getString = (string id) => (string)LocalizeDictionary.Instance.GetLocalizedObject(id, null, null);
        e.Model.Title = e.Model.ContentId switch
        {
            "Modules"           => getString("Lang:PlanArea_ModuleList"),
            "Products"          => getString("Lang:PlanArea_Products"),
            "BuildResources"    => getString("Lang:PlanArea_BuildResources"),
            "Storages"          => getString("Lang:PlanArea_Storages"),
            "StorageAssign"     => getString("Lang:PlanArea_StorageAssign"),
            "Summary"           => getString("Lang:PlanArea_Summary"),
            "Settings"          => getString("Lang:PlanArea_Settings"),
            _ => e.Model.Title
        };
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
            case nameof(_model.HasChanged):
                RaisePropertyChanged(nameof(HasChanged));
                RaisePropertyChanged(nameof(Title));
                break;

            case nameof(_model.Title):
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
        _model.PropertyChanged -= Model_PropertyChanged;
        LocalizeDictionary.Instance.PropertyChanged -= Instance_PropertyChanged;
        _model.Dispose();
        Summary.Dispose();
        Modules.Dispose();
        Products.Dispose();
        Resources.Dispose();
        Storages.Dispose();
    }
}

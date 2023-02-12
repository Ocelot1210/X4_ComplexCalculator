using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPFLocalizeExtension.Engine;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.WorkArea.UI.Menu.Tab;

namespace X4_ComplexCalculator.Main.WorkArea.UI;


/// <summary>
/// 単一の作業エリアのレイアウトを管理するクラス
/// </summary>
/// <remarks>ドッキング・ドッキング解除するとレイアウトがもとに戻る対策</remarks>
public sealed class LayoutManager : IDisposable
{
    /// <summary>
    /// レイアウト ID
    /// </summary>
    private readonly long _initialLayoutID;


    /// <summary>
    /// 前回のドッキングマネージャー
    /// </summary>
    private DockingManager? _prevDockingManager;


    /// <summary>
    /// 表示/非表示用メニューアイテム一覧
    /// </summary>
    public ObservableRangeCollection<VisiblityMenuItem> VisiblityMenuItems { get; } = new();


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="layoutID">レイアウト ID</param>
    public LayoutManager(long layoutID)
    {
        _initialLayoutID = layoutID;
        LocalizeDictionary.Instance.PropertyChanged += Instance_PropertyChanged;
    }


    /// <summary>
    /// 言語変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Instance_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LocalizeDictionary.Instance.Culture) && _prevDockingManager is not null)
        {
            // 表示メニューを初期化
            VisiblityMenuItems.Reset(_prevDockingManager.Layout.Descendents().OfType<LayoutAnchorable>().Select(x => new VisiblityMenuItem(x)));
        }
    }


    /// <summary>
    /// ロード時
    /// </summary>
    /// <param name="newDockingManager">新しい <see cref="DockingManager"/></param>
    public void OnLoaded(DockingManager newDockingManager)
    {
        if (_prevDockingManager is null)
        {
            OnCreated(newDockingManager);
        }
        else
        {
            OnParentChanged(newDockingManager);
        }
    }


    /// <summary>
    /// 初回作成時
    /// </summary>
    /// <param name="newDockingManager">新しい <see cref="DockingManager"/></param>
    private void OnCreated(DockingManager newDockingManager)
    {
        _prevDockingManager = newDockingManager;

        // 初期値として設定されたレイアウト ID で初期化する
        SetLayout(_initialLayoutID);

        VisiblityMenuItems.Reset(_prevDockingManager.Layout.Descendents().OfType<LayoutAnchorable>().Select(x => new VisiblityMenuItem(x)));
    }


    /// <summary>
    /// ドッキング・ドッキング解除した際に呼ばれる処理
    /// </summary>
    /// <param name="newDockingManager">新しい <see cref="DockingManager"/></param>
    private void OnParentChanged(DockingManager newDockingManager)
    {
        if (_prevDockingManager is null) throw new InvalidOperationException();

        // 現在のレイアウトを新しい DockingManager に引き継ぐ
        DeserializedLayout(newDockingManager, SerializeLayout(_prevDockingManager));

        _prevDockingManager = newDockingManager;
    }


    /// <summary>
    /// レイアウト ID を元に現在のレイアウトを変更する
    /// </summary>
    /// <param name="layoutID">レイアウト ID</param>
    public void SetLayout(long layoutID)
    {
        if (_prevDockingManager is null) return;

        var layoutExists = SettingDatabase.Instance.QuerySingle<bool>("SELECT count(*) FROM WorkAreaLayouts WHERE LayoutID = :layoutID", new { layoutID });
        if (layoutExists)
        {
            // DB から取得したレイアウトを適用
            var layout = SettingDatabase.Instance.QuerySingle<byte[]>("SELECT Layout FROM WorkAreaLayouts WHERE LayoutID = :layoutID", new { layoutID });
            if (layout is not null)
            {
                DeserializedLayout(_prevDockingManager, layout);

                // 表示メニューを初期化
                VisiblityMenuItems.Reset(_prevDockingManager.Layout.Descendents().OfType<LayoutAnchorable>().Select(x => new VisiblityMenuItem(x)));
            }
        }
    }


    /// <summary>
    /// 現在のレイアウトを DB に保存する
    /// </summary>
    /// <param name="layoutName">レイアウト名</param>
    /// <returns>レイアウトID</returns>
    public long SaveLayout(string layoutName)
    {
        if (_prevDockingManager is null) throw new InvalidOperationException();

        var id = SettingDatabase.Instance.GetLastLayoutID();

        var param = new
        {
            LayoutID = id,
            LayoutName = layoutName,
            Layout = SerializeLayout(_prevDockingManager),
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
        if (_prevDockingManager is null) throw new InvalidOperationException();

        var param = new
        {
            LayoutID = layoutID,
            Layout = SerializeLayout(_prevDockingManager),
        };

        const string SQL = "UPDATE WorkAreaLayouts SET Layout = :Layout WHERE LayoutID = :LayoutID";
        SettingDatabase.Instance.Execute(SQL, param);
    }


    /// <summary>
    /// シリアライズされたレイアウトを取得
    /// </summary>
    /// <param name="dockingManager">シリアライズ対象の <see cref="DockingManager"/></param>
    /// <returns>現在のレイアウトを表す UTF-8 XML のバイト配列</returns>
    private static byte[] SerializeLayout(DockingManager dockingManager)
    {
        // レイアウトをシリアライズ
        var serializer = new XmlLayoutSerializer(dockingManager);
        using var ms = new MemoryStream();
        serializer.Serialize(ms);
        ms.Position = 0;

        return ms.ToArray();
    }


    /// <summary>
    /// レイアウトをデシリアライズする
    /// </summary>
    /// <param name="dockingManager">デシリアライズ対象の <see cref="DockingManager"/></param>
    /// <param name="layout">レイアウトを表すデータ</param>
    private static void DeserializedLayout(DockingManager dockingManager, byte[] layout)
    {
        var serializer = new XmlLayoutSerializer(dockingManager);
        serializer.LayoutSerializationCallback += LayoutSerializeCallback;

        using var ms = new MemoryStream(layout, false);
        serializer.Deserialize(ms);
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


    /// <inheritdoc/>
    public void Dispose()
    {
        LocalizeDictionary.Instance.PropertyChanged -= Instance_PropertyChanged;
    }
}
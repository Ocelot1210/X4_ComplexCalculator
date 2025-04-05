using AvalonDock;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collections;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;
using X4_ComplexCalculator.Main.WorkArea.SaveDataWriters;
using X4_ComplexCalculator.Main.WorkArea.UI;
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
public sealed partial class WorkAreaViewModel : ObservableRecipient, IDisposable
{
    #region メンバ
    /// <summary>
    /// モデル
    /// </summary>
    private readonly WorkAreaModel _model;
    #endregion


    #region プロパティ
    /// <summary>
    /// 表示/非表示用メニューアイテム一覧
    /// </summary>
    public ObservableRangeCollection<VisiblityMenuItem> VisiblityMenuItems => LayoutManager.VisiblityMenuItems;


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
    public StationSettingInfo Settings => _model.StationData.Settings;


    /// <summary>
    /// タブのタイトル文字列
    /// </summary>
    public string Title => _model.Title;


    /// <summary>
    /// モジュールの内容に変更があったか
    /// </summary>
    public bool HasChanged => _model.HasChanged;


    /// <summary>
    /// 保存先ファイルパス
    /// </summary>
    public string SaveFilePath => _model.SaveFilePath;


    /// <summary>
    /// レイアウト管理
    /// </summary>
    public LayoutManager LayoutManager { get; }


    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    public ILocalizedMessageBox MessageBox { get; }

    

    public IWorkArea WorkArea => _model;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="layoutID">レイアウトID</param>
    /// <param name="messageBox">メッセージボックス表示用</param>
    /// <remarks>
    /// レイアウトIDが負の場合、レイアウトは指定されていない事にする
    /// </remarks>
    public WorkAreaViewModel(IMessenger messenger, long layoutID, ILocalizedMessageBox messageBox) : base(messenger)
    {
        _model                  = new(Messenger, new SQLiteSaveDataWriter(messageBox));
        LayoutManager           = new LayoutManager(layoutID);
        MessageBox              = messageBox;

        Summary                 = new(Messenger, _model.StationData);
        Modules                 = new(Messenger, _model.StationData, messageBox);
        Products                = new(Messenger, _model.StationData);
        Resources               = new(Messenger, _model.StationData);
        Storages                = new(Messenger, _model.StationData);
        StorageAssign           = new(Messenger, _model.StationData);

        Messenger.RegisterPropertyChangedMessage(this, static (WorkAreaModel x) => x.HasChanged, static (r, m) => r.OnPropertyChanged(nameof(HasChanged)));
        Messenger.RegisterPropertyChangedMessage(this, static (WorkAreaModel x) => x.Title,      static (r, m) => r.OnPropertyChanged(nameof(Title)));
    }


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
    public bool LoadFile(string path, IProgress<double> progress) => _model.Load(path, progress);


    /// <summary>
    /// ロード時
    /// </summary>
    [RelayCommand]
    private void OnLoaded(DockingManager dockingManager) => LayoutManager.OnLoaded(dockingManager);


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _model.Dispose();
        LayoutManager.Dispose();
        Summary.Dispose();
        Modules.Dispose();
        Products.Dispose();
        Resources.Dispose();
        Storages.Dispose();

        Messenger.UnregisterAll(this);
    }


    /// <summary>
    /// このインスタンスの <see cref="IMessenger"/> を取得する
    /// </summary>
    public IMessenger GetMessenger() => Messenger;
}

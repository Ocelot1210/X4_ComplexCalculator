using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.ComponentModel;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.Main.WorkArea.SaveDataReaders;
using X4_ComplexCalculator.Main.WorkArea.SaveDataWriters;
using X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

namespace X4_ComplexCalculator.Main.WorkArea;

/// <summary>
/// 作業エリア用Model
/// </summary>
sealed partial class WorkAreaModel : ObservableRecipientEx, IDisposable, IWorkArea
{
    #region メンバ
    /// <summary>
    /// タイトル文字列
    /// </summary>
    private string _title = "";


    /// <summary>
    /// 保存ファイル書き込み用
    /// </summary>
    private readonly ISaveDataWriter _saveDataWriter;
    #endregion


    #region プロパティ
    /// <summary>
    /// タイトル文字列
    /// </summary>
    public string Title
    {
        get => string.IsNullOrEmpty(_title) ? "no title*" : (HasChanged) ? $"{_title}*" : _title;
        set => SetProperty(ref _title, value);
    }


    /// <summary>
    /// 計算機で使用するステーション用データ
    /// </summary>
    public IStationData StationData { get; }


    /// <summary>
    /// 変更されたか
    /// </summary>
    [ObservableProperty]
    public partial bool HasChanged { get; set; }


    /// <summary>
    /// 保存先ファイルパス
    /// </summary>
    public string SaveFilePath => _saveDataWriter.SaveFilePath;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public WorkAreaModel(IMessenger messenger, ISaveDataWriter saveDataWriter) : base(messenger, true)
    {
        _saveDataWriter = saveDataWriter;
        StationData = new StationData(Messenger);

        HasChanged = true;

        Messenger.RegisterPropertyChangedMessage(this, static (ModulesGridItem x)        => x.EditStatus, OnEditStatusChanged);
        Messenger.RegisterPropertyChangedMessage(this, static (ProductsGridItem x)       => x.EditStatus, OnEditStatusChanged);
        Messenger.RegisterPropertyChangedMessage(this, static (BuildResourcesGridItem x) => x.EditStatus, OnEditStatusChanged);
        Messenger.RegisterPropertyChangedMessage(this, static (StorageAssignGridItem x)  => x.EditStatus, OnEditStatusChanged);

        Messenger.RegisterPropertyChangedMessage(this, static (StationSettings x)  => x.IsHeadquarters, (r, m) => HasChanged = true);
        Messenger.RegisterPropertyChangedMessage(this, static (StationSettings x)  => x.Sunlight,       (r, m) => HasChanged = true);
        Messenger.RegisterPropertyChangedMessage(this, static (WorkforceManager x) => x.Actual,         (r, m) => HasChanged = true);
    }


    /// <summary>
    /// 編集状態変更時
    /// </summary>
    private void OnEditStatusChanged(WorkAreaModel recipient, PropertyChangedMessage<EditStatus> message)
    {
        if (message.NewValue == EditStatus.Edited)
        {
            HasChanged = true;
        }
    }


    partial void OnHasChangedChanged(bool value)
    {
        if (value)
        {
            // 変更検知イベントを購読解除
            StationData.Settings.PropertyChanged -= OnPropertyChanged;
            StationData.Settings.Workforce.PropertyChanged -= OnPropertyChanged;
        }
        else
        {
            // 変更検知イベントを購読
            StationData.Settings.PropertyChanged += OnPropertyChanged;
            StationData.Settings.Workforce.PropertyChanged += OnPropertyChanged;
        }
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        // 変更検知イベントを購読解除
        StationData.Settings.PropertyChanged -= OnPropertyChanged;
        StationData.Settings.Workforce.PropertyChanged -= OnPropertyChanged;

        Messenger.UnregisterPropertyChangedMessage(this, static (ModulesGridItem x)        => x.EditStatus);
        Messenger.UnregisterPropertyChangedMessage(this, static (ProductsGridItem x)       => x.EditStatus);
        Messenger.UnregisterPropertyChangedMessage(this, static (BuildResourcesGridItem x) => x.EditStatus);
        Messenger.UnregisterPropertyChangedMessage(this, static (StorageAssignGridItem x)  => x.EditStatus);

        Messenger.UnregisterPropertyChangedMessage(this, static (StationSettings x)  => x.IsHeadquarters);
        Messenger.UnregisterPropertyChangedMessage(this, static (StationSettings x)  => x.Sunlight);
        Messenger.UnregisterPropertyChangedMessage(this, static (WorkforceManager x) => x.Actual);
    }


    /// <summary>
    /// コレクションのプロパティに変更があった場合
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        string[] names =
        {
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
        if (_saveDataWriter.Save(this))
        {
            HasChanged = false;
        }
    }


    /// <summary>
    /// 名前を指定して保存
    /// </summary>
    public void SaveAs()
    {
        if (_saveDataWriter.SaveAs(this))
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
        var reader = SaveDataReaderFactory.CreateSaveDataReader(path, Messenger, this);
        var ret = false;

        // 読み込み成功？
        if (reader.Load(progress))
        {
            _saveDataWriter.SaveFilePath = path;
            HasChanged = false;
            ret = true;
        }

        return ret;
    }
}

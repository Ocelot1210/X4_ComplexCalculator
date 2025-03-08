using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;

/// <summary>
/// 保管庫割当用Gridの1レコード分
/// </summary>
public sealed partial class StorageAssignGridItem : ObservableRecipientEx, IDisposable, IEditable
{
    /// <summary>
    /// ウェア大きさ
    /// </summary>
    private readonly long _volume;


    #region プロパティ
    /// <summary>
    /// 保管庫容量情報
    /// </summary>
    public StorageCapacityInfo CapacityInfo { get; }


    /// <summary>
    /// カーゴ種別ID
    /// </summary>
    public string TransportTypeID { get; }


    /// <summary>
    /// カーゴ種別名
    /// </summary>
    public string TransportTypeName { get; }


    /// <summary>
    /// 階級
    /// </summary>
    public long Tier { get; }


    /// <summary>
    /// ウェアID
    /// </summary>
    public string WareID { get; }


    /// <summary>
    /// ウェア名
    /// </summary>
    public string WareName { get; }


    /// <summary>
    /// 割当数量
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AllocCapacity))]
    [NotifyPropertyChangedFor(nameof(StorageStatus))]
    public partial long AllocCount { get; set; }


    /// <summary>
    /// 保管庫状態
    /// </summary>
    public int StorageStatus => (AfterCount < 0) ? -1 : (AfterCount <= AllocCount) ? 0 : 1;


    /// <summary>
    /// 割当容量
    /// </summary>
    public long AllocCapacity => AllocCount * _volume;


    /// <summary>
    /// 割当可能容量最大
    /// </summary>
    public long MaxAllocableCount => (CapacityInfo.FreeCapacity + AllocCapacity) / _volume;


    /// <summary>
    /// 残り割当可能容量
    /// </summary>
    public long AllocableCount => CapacityInfo.FreeCapacity / _volume;


    /// <summary>
    /// 1時間あたりの生産量
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AfterCount))]
    [NotifyPropertyChangedFor(nameof(StorageStatus))]
    public partial long ProductPerHour { get; set; }


    /// <summary>
    /// 指定時間
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AfterCount))]
    [NotifyPropertyChangedFor(nameof(StorageStatus))]
    public partial long Hour { get; set; }


    /// <summary>
    /// 指定時間後の個数
    /// </summary>
    public long AfterCount => ProductPerHour * Hour;


    /// <summary>
    /// 編集状態
    /// </summary>
    [ObservableProperty]
    public partial EditStatus EditStatus { get; set; }
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="ware">ウェア</param>
    /// <param name="capacityInfo">保管庫容量情報</param>
    /// <param name="productPerHour">1時間あたりのウェア生産量</param>
    /// <param name="hour">指定時間</param>
    public StorageAssignGridItem(IMessenger messenger, IWare ware, StorageCapacityInfo capacityInfo, long productPerHour, long hour) : base(messenger, false)
    {
        WareID = ware.ID;
        WareName = ware.Name;
        Tier = ware.WareGroup.Tier;

        TransportTypeID = ware.TransportType.TransportTypeID;
        TransportTypeName = ware.TransportType.Name;

        _volume = ware.Volume;
        CapacityInfo = capacityInfo;
        ProductPerHour = productPerHour;
        Hour = hour;

        Messenger.RegisterPropertyChangedMessage(this, static (StorageCapacityInfo x) => x.FreeCapacity, OnFreeCapacityChanged);

        IsActive = true;
    }


    /// <summary>
    /// 全体の保管庫容量変更時
    /// </summary>
    private void OnFreeCapacityChanged(StorageAssignGridItem recipient, PropertyChangedMessage<long> message)
    {
        // 容量変更された保管庫種別が自分と同じならプロパティ更新
        if (message.Sender == CapacityInfo)
        {
            OnPropertyChanged(nameof(AllocableCount));
            OnPropertyChanged(nameof(MaxAllocableCount));
        }
    }


    /// <summary>
    /// <see cref="AllocCount"/> 変更時
    /// </summary>
    partial void OnAllocCountChanged(long oldValue, long newValue)
    {
        CapacityInfo.UsedCapacity += (newValue - oldValue) * _volume;
        EditStatus = EditStatus.Edited;
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        Messenger.UnregisterPropertyChangedMessage(this, static (StorageCapacityInfo x) => x.FreeCapacity);
    }
}

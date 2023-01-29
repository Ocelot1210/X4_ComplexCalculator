using Prism.Mvvm;
using System;
using System.ComponentModel;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;

/// <summary>
/// 保管庫割当用Gridの1レコード分
/// </summary>
public sealed class StorageAssignGridItem : BindableBase, IDisposable, IEditable
{
    #region メンバ
    /// <summary>
    /// 指定時間
    /// </summary>
    private long _hour;

    /// <summary>
    /// 割当容量
    /// </summary>
    private long _allocCount;


    /// <summary>
    /// 1時間あたりの生産量
    /// </summary>
    private long _productPerHour;


    /// <summary>
    /// 編集状態
    /// </summary>
    private EditStatus _editStatus = EditStatus.Unedited;
    #endregion


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
    /// ウェア大きさ
    /// </summary>
    public long Volume;


    /// <summary>
    /// 割当数量
    /// </summary>
    public long AllocCount
    {
        get => _allocCount;
        set
        {
            var prevCount = _allocCount;

            if (SetProperty(ref _allocCount, value))
            {
                RaisePropertyChanged(nameof(AllocCapacity));
                RaisePropertyChanged(nameof(StorageStatus));
                CapacityInfo.UsedCapacity += (value - prevCount) * Volume;
                EditStatus = EditStatus.Edited;
            }
        }
    }


    /// <summary>
    /// 保管庫状態
    /// </summary>
    public int StorageStatus => (AfterCount < 0) ? -1 :
                                (AfterCount <= AllocCount) ? 0 : 1;


    /// <summary>
    /// 割当容量
    /// </summary>
    public long AllocCapacity => AllocCount * Volume;


    /// <summary>
    /// 割当可能容量最大
    /// </summary>
    public long MaxAllocableCount => (CapacityInfo.FreeCapacity + AllocCapacity) / Volume;


    /// <summary>
    /// 残り割当可能容量
    /// </summary>
    public long AllocableCount => CapacityInfo.FreeCapacity / Volume;


    /// <summary>
    /// 1時間あたりの生産量
    /// </summary>
    public long ProductPerHour
    {
        get => _productPerHour;
        set
        {
            if (SetProperty(ref _productPerHour, value))
            {
                RaisePropertyChanged(nameof(AfterCount));
                RaisePropertyChanged(nameof(StorageStatus));
            }
        }
    }


    /// <summary>
    /// 指定時間
    /// </summary>
    public long Hour
    {
        get => _hour;
        set
        {
            if (SetProperty(ref _hour, value))
            {
                RaisePropertyChanged(nameof(AfterCount));
                RaisePropertyChanged(nameof(StorageStatus));
            }
        }
    }


    /// <summary>
    /// 指定時間後の個数
    /// </summary>
    public long AfterCount => ProductPerHour * Hour;


    /// <summary>
    /// 編集状態
    /// </summary>
    public EditStatus EditStatus
    {
        get => _editStatus;
        set => SetProperty(ref _editStatus, value);
    }
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="ware">ウェア</param>
    /// <param name="capacityInfo">保管庫容量情報</param>
    /// <param name="productPerHour">1時間あたりのウェア生産量</param>
    /// <param name="hour">指定時間</param>
    public StorageAssignGridItem(IWare ware, StorageCapacityInfo capacityInfo, long productPerHour, long hour)
    {
        WareID = ware.ID;
        WareName = ware.Name;
        Tier = ware.WareGroup.Tier;

        TransportTypeID = ware.TransportType.TransportTypeID;
        TransportTypeName = ware.TransportType.Name;

        Volume = ware.Volume;
        CapacityInfo = capacityInfo;
        CapacityInfo.PropertyChanged += CapacityInfo_PropertyChanged;

        ProductPerHour = productPerHour;
        Hour = hour;
    }

    /// <summary>
    /// 保管庫容量プロパティ変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CapacityInfo_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(StorageCapacityInfo.FreeCapacity):
                RaisePropertyChanged(nameof(AllocableCount));
                RaisePropertyChanged(nameof(MaxAllocableCount));
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
        CapacityInfo.PropertyChanged -= CapacityInfo_PropertyChanged;
    }
}

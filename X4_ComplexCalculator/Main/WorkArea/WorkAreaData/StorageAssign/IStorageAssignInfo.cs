﻿using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StorageAssign;

/// <summary>
/// 保管庫割当情報用インターフェイス
/// </summary>
public interface IStorageAssignInfo
{
    /// <summary>
    /// 保管庫割当情報
    /// </summary>
    public ObservablePropertyChangedCollection<StorageAssignGridItem> StorageAssign { get; }
}

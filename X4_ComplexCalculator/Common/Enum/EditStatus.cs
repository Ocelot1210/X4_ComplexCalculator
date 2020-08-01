using System;

namespace X4_ComplexCalculator.Common.Enum
{
    /// <summary>
    /// 編集状態用列挙体
    /// </summary>
    [Flags]
    public enum EditStatus
    {
        Unedited        = 0b_0000_0000,             // 未編集
        Edited          = 0b_0000_0001,             // 編集された
        Saved           = 0b_0000_0010,             // 保存された
        EditAndSaved    = Edited | Saved,           // 編集して保存された
    }
}

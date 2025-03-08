using System;

namespace X4_ComplexCalculator.DB.X4DB.Interfaces;

/// <summary>
/// サイズ情報用インターフェイス
/// </summary>
public interface IX4Size : IComparable<IX4Size>
{
    #region プロパティ
    /// <summary>
    /// サイズID
    /// </summary>
    public string SizeID { get; }


    /// <summary>
    /// サイズ名
    /// </summary>
    public string Name { get; }


    /// <summary>
    /// サイズ(比較用)
    /// </summary>
    public int Size { get; }
    #endregion



    /// <inheritdoc/>
    int IComparable<IX4Size>.CompareTo(IX4Size? other)
    {
        if (other is null)
        {
            return 1;
        }

        return this.Size - other.Size;
    }
}

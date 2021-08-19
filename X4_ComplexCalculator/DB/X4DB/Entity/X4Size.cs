using System;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// X4のサイズ情報用クラス
    /// </summary>
    /// <param name="sizeID">サイズID</param>
    /// <param name="name">サイズ名</param>
    public sealed record X4Size(string SizeID, string Name) : IX4Size
    {
        #region プロパティ
        /// <inheritdoc/>
        public int Size { get; } = SizeID switch
        {
            "extrasmall" => 0,
            "small" => 1,
            "medium" => 2,
            "large" => 3,
            "extralarge" => 4,
            _ => throw new NotSupportedException($"SizeID \"{SizeID}\" is not supported.")
        };
        #endregion
    }
}

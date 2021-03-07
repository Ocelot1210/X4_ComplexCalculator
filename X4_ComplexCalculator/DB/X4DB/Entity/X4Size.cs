using System;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// X4のサイズ情報用クラス
    /// </summary>
    public class X4Size : IX4Size
    {
        #region プロパティ
        /// <inheritdoc/>
        public string SizeID { get; }


        /// <inheritdoc/>
        public string Name { get; }


        /// <inheritdoc/>
        public int Size { get; }
        #endregion  


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sizeID">サイズID</param>
        /// <param name="name">サイズ名</param>
        public X4Size(string sizeID, string name)
        {
            SizeID = sizeID;
            Name = name;

            Size = SizeID switch
            {
                "extrasmall" => 0,
                "small" => 1,
                "medium" => 2,
                "large" => 3,
                "extralarge" => 4,
                _ => throw new NotSupportedException($"SizeID \"{SizeID}\" is not supported.")
            };
        }

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(SizeID);


        /// <inheritdoc/>
        public override string ToString() => Name;
    }
}

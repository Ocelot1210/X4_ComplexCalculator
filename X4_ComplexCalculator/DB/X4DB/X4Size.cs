using System;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// サイズ管理用クラス
    /// </summary>
    public class X4Size : IComparable
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
        /// 比較用の値
        /// </summary>
        private readonly int _CompareValue;
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

            _CompareValue = SizeID switch
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
        public override bool Equals(object? obj) => obj is X4Size other && Equals(other);


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(X4Size? other) => other is not null && SizeID == other.SizeID;


        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(SizeID);


        /// <inheritdoc/>
        public override string ToString() => Name;


        /// <inheritdoc/>
        public int CompareTo(object? obj)
        {
            if (obj is null)
            {
                return 1;
            }

            if (obj is not X4Size size)
            {
                throw new ArgumentException();
            }

            return _CompareValue - size._CompareValue;
        }
    }
}

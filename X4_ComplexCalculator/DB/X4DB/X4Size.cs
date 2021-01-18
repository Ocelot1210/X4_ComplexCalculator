using System;
using System.Collections.Generic;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// サイズ管理用クラス
    /// </summary>
    public class X4Size : IComparable
    {
        #region スタティックメンバ
        /// <summary>
        /// サイズ一覧
        /// </summary>
        private readonly static Dictionary<string, X4Size> _Sizes = new();
        #endregion


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
        private int _CompareValue;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sizeID">サイズID</param>
        /// <param name="name">サイズ名</param>
        private X4Size(string sizeID, string name)
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


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _Sizes.Clear();

            const string sql = "SELECT SizeID, Name FROM Size";
            foreach (var size in X4Database.Instance.Query<X4Size>(sql))
            {
                _Sizes.Add(size.SizeID, size);
            }
        }


        /// <summary>
        /// サイズIDに対応するサイズを取得
        /// </summary>
        /// <param name="sizeID">サイズID</param>
        /// <returns>サイズ</returns>
        public static X4Size Get(string sizeID) => _Sizes[sizeID];


        /// <summary>
        /// サイズIDに対応するサイズの取得を試みる
        /// </summary>
        /// <param name="sizeID">サイズID</param>
        /// <returns>サイズIDに対応するサイズ</returns>
        public static X4Size? TryGet(string sizeID) => _Sizes.TryGetValue(sizeID, out var ret) ? ret : null;



        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is X4Size tgt && tgt.SizeID == SizeID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(SizeID);


        /// <summary>
        /// 文字列化
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name;


        /// <summary>
        /// 大小比較
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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

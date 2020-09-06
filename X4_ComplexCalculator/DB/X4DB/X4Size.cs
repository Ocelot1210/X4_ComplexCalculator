using System;
using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// サイズ管理用クラス
    /// </summary>
    public class X4Size
    {
        #region スタティックメンバ
        /// <summary>
        /// サイズ一覧
        /// </summary>
        private readonly static Dictionary<string, X4Size> _Sizes = new Dictionary<string, X4Size>();
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
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _Sizes.Clear();
            X4Database.Instance.ExecQuery("SELECT SizeID, Name FROM Size", (dr, args) =>
            {
                var id = (string)dr["SizeID"];
                var name = (string)dr["Name"];

                _Sizes.Add(id, new X4Size(id, name));
            });
        }


        /// <summary>
        /// サイズIDに対応するサイズを取得
        /// </summary>
        /// <param name="sizeID">サイズID</param>
        /// <returns>サイズ</returns>
        public static X4Size Get(string sizeID) => _Sizes[sizeID];


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
    }
}

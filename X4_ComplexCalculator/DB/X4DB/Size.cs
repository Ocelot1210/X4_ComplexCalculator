using System;
using System.Data.SQLite;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// サイズ管理用クラス
    /// </summary>
    public class Size : IComparable
    {
        /// <summary>
        /// サイズID
        /// </summary>
        public string SizeID { get; }

        /// <summary>
        /// サイズ名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sizeID">サイズID</param>
        public Size(string sizeID)
        {
            SizeID = sizeID;
            string name = "";
            DBConnection.X4DB.ExecQuery($"SELECT * FROM Size WHERE SizeID = '{sizeID}'", (SQLiteDataReader dr, object[] args) => { name = dr["Name"].ToString(); });
            Name = name;
        }


        /// <summary>
        /// オブジェクト比較
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            return SizeID.CompareTo(obj is Size);
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is Size tgt && tgt.SizeID == SizeID;
        }

        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode()
        {
            return SizeID.GetHashCode();
        }
    }
}

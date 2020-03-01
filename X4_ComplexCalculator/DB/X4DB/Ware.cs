using System;
using System.Data.SQLite;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// ウェア情報管理用クラス
    /// </summary>
    public class Ware : IComparable
    {
        #region プロパティ
        /// <summary>
        /// ウェアID
        /// </summary>
        public string WareID { private set; get; }

        /// <summary>
        /// ウェア名
        /// </summary>
        public string Name { private set; get; }


        /// <summary>
        /// ウェアグループ
        /// </summary>
        public WareGroup WareGroup { private set; get; }


        /// <summary>
        /// カーゴ種別(輸送種別)
        /// </summary>
        public TransportType TransportType { private set; get; }


        /// <summary>
        /// コンテナサイズ
        /// </summary>
        public long Volume { private set; get; }


        /// <summary>
        /// 最低価格
        /// </summary>
        public long MinPrice { private set; get; }


        /// <summary>
        /// 最高価格
        /// </summary>
        public long MaxPrice { private set; get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">ウェアID</param>
        public Ware(string id)
        {
            WareID = id;
            DBConnection.X4DB.ExecQuery(
                $"SELECT * FROM Ware WHERE WareID = '{WareID}'",
                (SQLiteDataReader dr, object[] args) =>
                {
                    Name = dr["Name"].ToString();
                    Volume = (long)dr["Volume"];
                    MinPrice = (long)dr["MinPrice"];
                    MaxPrice = (long)dr["MaxPrice"];
                    WareGroup = new WareGroup(dr["WareGroupID"].ToString());
                    TransportType = new TransportType(dr["TransportTypeID"].ToString());
                });
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            return WareID.CompareTo(obj);
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is Ware tgt && tgt.WareID == WareID;
        }

        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode()
        {
            return WareID.GetHashCode();
        }
    }
}

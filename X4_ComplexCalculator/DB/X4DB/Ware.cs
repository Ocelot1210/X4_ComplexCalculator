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
        public string WareID { get; }

        /// <summary>
        /// ウェア名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// ウェアグループ
        /// </summary>
        public WareGroup WareGroup { get; }


        /// <summary>
        /// カーゴ種別(輸送種別)
        /// </summary>
        public TransportType TransportType { get; }


        /// <summary>
        /// コンテナサイズ
        /// </summary>
        public long Volume { get; }


        /// <summary>
        /// 最低価格
        /// </summary>
        public long MinPrice { get; }


        /// <summary>
        /// 最高価格
        /// </summary>
        public long MaxPrice { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        public Ware(string wareID)
        {
            WareID = wareID;
            var name     = "";
            var volume   = 0L;
            var minPrice = 0L;
            var maxPrice = 0L;
            WareGroup wareGroup = null;
            TransportType transportType = null;

            DBConnection.X4DB.ExecQuery(
                $"SELECT * FROM Ware WHERE WareID = '{WareID}'",
                (SQLiteDataReader dr, object[] args) =>
                {
                    name          = dr["Name"].ToString();
                    volume        = (long)dr["Volume"];
                    minPrice      = (long)dr["MinPrice"];
                    maxPrice      = (long)dr["MaxPrice"];
                    wareGroup     = new WareGroup(dr["WareGroupID"].ToString());
                    transportType = new TransportType(dr["TransportTypeID"].ToString());
                });

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid ware id.", nameof(wareID));
            }

            Name          = name;
            Volume        = volume;
            MinPrice      = minPrice;
            MaxPrice      = maxPrice;
            WareGroup     = wareGroup;
            TransportType = transportType;
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

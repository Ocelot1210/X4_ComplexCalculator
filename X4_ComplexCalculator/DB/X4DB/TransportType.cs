using System;
using System.Data.SQLite;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// カーゴタイプ(輸送種別)管理用クラス
    /// </summary>
    public class TransportType
    {
        #region プロパティ
        /// <summary>
        /// カーゴ種別ID
        /// </summary>
        public string TransportTypeID { get; }


        /// <summary>
        /// ウェアグループ名
        /// </summary>
        public string Name { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="transportTypeID">カーゴタイプID</param>
        public TransportType(string transportTypeID)
        {
            TransportTypeID = transportTypeID;
            string name = "";
            DBConnection.X4DB.ExecQuery(
                $"SELECT Name FROM TransportType WHERE TransportTypeID = '{TransportTypeID}'",
                (SQLiteDataReader dr, object[] args) =>
                {
                    name = (string)dr["Name"];
                });

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid transport type id.", nameof(transportTypeID));
            }

            Name = name;
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return obj is TransportType tgt && tgt.TransportTypeID == TransportTypeID;
        }

        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode()
        {
            return TransportTypeID.GetHashCode();
        }
    }
}

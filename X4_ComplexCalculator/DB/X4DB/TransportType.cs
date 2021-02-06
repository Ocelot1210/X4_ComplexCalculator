using System;


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
        /// カーゴ種別名
        /// </summary>
        public string Name { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="transportTypeID">カーゴ種別ID</param>
        /// <param name="name">カーゴ種別名</param>
        public TransportType(string transportTypeID, string name)
        {
            TransportTypeID = transportTypeID;
            Name = name;
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is TransportType other && Equals(other);


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(TransportType other) => TransportTypeID == other.TransportTypeID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(TransportTypeID);
    }
}

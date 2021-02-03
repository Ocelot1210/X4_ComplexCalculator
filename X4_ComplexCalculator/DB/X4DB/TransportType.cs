using System;
using System.Collections.Generic;


namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// カーゴタイプ(輸送種別)管理用クラス
    /// </summary>
    public class TransportType
    {
        #region スタティックメンバ
        /// <summary>
        /// カーゴタイプ一覧
        /// </summary>
        private readonly static Dictionary<string, TransportType> _TransportTypes = new();
        #endregion


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
        private TransportType(string transportTypeID, string name)
        {
            TransportTypeID = transportTypeID;
            Name = name;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _TransportTypes.Clear();

            const string sql = "SELECT TransportTypeID, Name FROM TransportType";
            foreach (var item in X4Database.Instance.Query<TransportType>(sql))
            {
                _TransportTypes.Add(item.TransportTypeID, item);
            }
        }


        /// <summary>
        /// カーゴ種別IDに対応するカーゴ種別を取得する
        /// </summary>
        /// <param name="transportTypeID">カーゴ種別ID</param>
        /// <returns>カーゴ種別</returns>
        public static TransportType Get(string transportTypeID) => _TransportTypes[transportTypeID];


        /// <summary>
        /// 全カーゴ種別を取得する
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TransportType> GetAll() => _TransportTypes.Values;


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

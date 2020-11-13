using System;
using System.Collections.Generic;


namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// ウェア情報管理用クラス
    /// </summary>
    public class Ware
    {
        #region スタティックメンバ
        /// <summary>
        /// ウェア一覧
        /// </summary>
        private readonly static Dictionary<string, Ware> _Wares = new();
        #endregion


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
        /// ウェア種別
        /// </summary>
        public WareGroup WareGroup { get; }


        /// <summary>
        /// カーゴ種別
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
        /// <param name="name">ウェア名</param>
        /// <param name="wareGroup">ウェア種別</param>
        /// <param name="transportType">カーゴ種別</param>
        /// <param name="volume">大きさ</param>
        /// <param name="minPrice">最低価格</param>
        /// <param name="maxPrice">最高価格</param>
        private Ware(string wareID, string name, WareGroup wareGroup, TransportType transportType, long volume, long minPrice, long maxPrice)
        {
            WareID = wareID;
            Name = name;
            WareGroup = wareGroup;
            TransportType = transportType;
            Volume = volume;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _Wares.Clear();
            X4Database.Instance.ExecQuery($"SELECT * FROM Ware", (dr, args) =>
            {
                var id = (string)dr["WareID"];
                var name = (string)dr["Name"];
                var volume = (long)dr["Volume"];
                var minPrice = (long)dr["MinPrice"];
                var maxPrice = (long)dr["MaxPrice"];
                var wareGroup = WareGroup.Get((string)dr["WareGroupID"]);
                var transportType = TransportType.Get((string)dr["TransportTypeID"]);

                _Wares.Add(id, new Ware(id, name, wareGroup, transportType, volume, minPrice, maxPrice));
            });
        }


        /// <summary>
        /// ウェアIDに対応するウェアを取得する
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <returns>ウェア</returns>
        public static Ware Get(string wareID) => _Wares[wareID];


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is Ware tgt && tgt.WareID == WareID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(WareID);
    }
}

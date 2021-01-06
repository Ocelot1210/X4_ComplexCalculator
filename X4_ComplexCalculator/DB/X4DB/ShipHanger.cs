using System;
using System.Collections.Generic;
using System.Text;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 艦船のハンガー情報を管理するクラス
    /// </summary>
    public class ShipHanger
    {
        #region スタティックメンバ
        /// <summary>
        /// 艦船のハンガー一覧
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, ShipHanger>> _ShipHangers = new();


        /// <summary>
        /// 空のハンガー情報(ダミー用)
        /// </summary>
        private readonly static IReadOnlyDictionary<string, ShipHanger> _EmptyHanger = new Dictionary<string, ShipHanger>();
        #endregion


        #region プロパティ
        /// <summary>
        /// 艦船ID
        /// </summary>
        public string ShipID { get; }


        /// <summary>
        /// 発着パッドのサイズ
        /// </summary>
        public X4Size Size { get; }


        /// <summary>
        /// 発着パッド数
        /// </summary>
        public long Count { get; }


        /// <summary>
        /// 機体格納数
        /// </summary>
        public long Capacity { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="shipID">艦船ID</param>
        /// <param name="sizeID">発着パッドのサイズID</param>
        /// <param name="count">発着パッド数</param>
        /// <param name="capacity">機体格納数</param>
        private ShipHanger(string shipID, string sizeID, long count, long capacity)
        {
            ShipID = shipID;
            Size = X4Size.Get(sizeID);
            Count = count;
            Capacity = capacity;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _ShipHangers.Clear();

            const string sql = "SELECT ShipID, SizeID, Count, Capacity FROM ShipHanger";
            foreach (var hanger in X4Database.Instance.Query<ShipHanger>(sql))
            {
                if (!_ShipHangers.ContainsKey(hanger.ShipID))
                {
                    _ShipHangers.Add(hanger.ShipID, new Dictionary<string, ShipHanger>());
                }

                _ShipHangers[hanger.ShipID].Add(hanger.Size.SizeID, hanger);
            }
        }


        /// <summary>
        /// 艦船のハンガー情報を取得する
        /// </summary>
        /// <param name="shipID">艦船ID</param>
        /// <returns>艦船IDに対応する艦船のハンガー情報</returns>
        public static IReadOnlyDictionary<string, ShipHanger> Get(string shipID) => _ShipHangers.TryGetValue(shipID, out var ret)?  ret : _EmptyHanger;
    }
}

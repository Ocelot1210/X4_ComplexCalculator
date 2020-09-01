using System;
using System.Collections.Generic;


namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// ウェア種別(グループ)管理用クラス
    /// </summary>
    public class WareGroup
    {
        #region スタティックメンバ
        /// <summary>
        /// ウェア種別一覧
        /// </summary>
        private static readonly Dictionary<string, WareGroup> _WareGroups = new Dictionary<string, WareGroup>();
        #endregion

        #region プロパティ
        /// <summary>
        /// ウェアグループID
        /// </summary>
        public string WareGroupID { get; }


        /// <summary>
        /// ウェアグループ名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 階級
        /// </summary>
        public long Tier { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareGroupID">ウェアグループID</param>
        /// <param name="name">ウェアグループ名</param>
        /// <param name="Tier">階級</param>
        private WareGroup(string wareGroupID, string name, long tier)
        {
            WareGroupID = wareGroupID;
            Name = name;
            Tier = tier;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _WareGroups.Clear();
            X4Database.Instance.ExecQuery($"SELECT WareGroupID, Name, Tier FROM WareGroup", (dr, args) =>
            {
                var id = (string)dr["WareGroupID"];
                var name = (string)dr["Name"];
                var tier = (long)dr["Tier"];

                _WareGroups.Add(id, new WareGroup(id, name, tier));
            });
        }


        /// <summary>
        /// ウェア種別IDに対応するウェア種別を取得する
        /// </summary>
        /// <param name="wareGroupID">ウェア種別ID</param>
        /// <returns>ウェア種別</returns>
        public static WareGroup Get(string wareGroupID) => _WareGroups[wareGroupID];


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is WareGroup tgt && tgt.WareGroupID == WareGroupID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(WareGroupID);
    }
}

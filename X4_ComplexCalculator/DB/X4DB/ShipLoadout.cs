using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 艦船のロードアウト情報
    /// </summary>
    public class ShipLoadout
    {
        #region スタティックメンバ
        /// <summary>
        /// 艦船のロードアウト情報一覧
        /// </summary>
        private static readonly Dictionary<string, IReadOnlyList<ShipLoadout>> _ShipLoadouts = new();

        /// <summary>
        /// ダミー用のロードアウト情報
        /// </summary>
        private static readonly IReadOnlyList<ShipLoadout> _EmptyLoadouts = Array.Empty<ShipLoadout>();
        #endregion


        #region メンバ
        /// <summary>
        /// 装備品ID
        /// </summary>
        private readonly string _EquipmentID;
        #endregion



        #region プロパティ
        /// <summary>
        /// 艦船ID
        /// </summary>
        public string ID { get; }


        /// <summary>
        /// ロードアウトID
        /// </summary>
        public string LoadoutID { get; }


        /// <summary>
        /// 装備品
        /// </summary>
        public Equipment Equipment => Ware.Get<Equipment>(_EquipmentID);


        /// <summary>
        /// グループ名
        /// </summary>
        public string GroupName { get; }


        /// <summary>
        /// 個数
        /// </summary>
        public long Count { get; }
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="shipID">艦船ID</param>
        /// <param name="loadoutID">ロードアウトID</param>
        /// <param name="macroName">マクロ名</param>
        /// <param name="groupName">グループ名</param>
        /// <param name="count">個数</param>
        private ShipLoadout(string shipID, string loadoutID, string macroName, string groupName, long count)
        {
            ID = shipID;
            LoadoutID = loadoutID;
            _EquipmentID = X4Database.Instance.QuerySingle<string>("SELECT EquipmentID FROM Equipment WHERE MacroName = :MacroName", new { MacroName = macroName });
            GroupName = groupName;
            Count = count;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _ShipLoadouts.Clear();

            const string sql = "SELECT ShipID, LoadoutID, MacroName, GroupName, Count FROM ShipLoadout";
            var items = X4Database.Instance.Query<ShipLoadout>(sql)
                .GroupBy(x => x.ID);

            foreach (var item in items)
            {
                _ShipLoadouts.Add(item.Key, item.ToArray());
            }
        }



        /// <summary>
        /// 艦船IDに対応する装備情報を取得する
        /// </summary>
        /// <param name="shipID">艦船ID</param>
        /// <returns>艦船IDに対応する装備情報</returns>
        public static IReadOnlyList<ShipLoadout> Get(string shipID)
            => _ShipLoadouts.TryGetValue(shipID, out var ret) ? ret : _EmptyLoadouts;
    }
}

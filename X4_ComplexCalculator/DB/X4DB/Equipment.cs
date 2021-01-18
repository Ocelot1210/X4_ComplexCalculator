using System;
using System.Collections.Generic;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 装備品管理用クラス
    /// </summary>
    public class Equipment : Ware
    {
        #region スタティックメンバ
        /// <summary>
        /// 装備一覧
        /// </summary>
        private readonly static Dictionary<string, Equipment> _Equipments = new();
        #endregion


        #region プロパティ
        /// <summary>
        /// マクロ名
        /// </summary>
        public string Macro { get; }


        /// <summary>
        /// 装備種別
        /// </summary>
        public EquipmentType EquipmentType { get; }


        /// <summary>
        /// 船体値
        /// </summary>
        public long Hull { get; }


        /// <summary>
        /// 船体値が統合されているか
        /// </summary>
        public bool HullIntegrated { get; }


        /// <summary>
        /// Mk
        /// </summary>
        public long Mk { get; }


        /// <summary>
        /// 製造種族
        /// </summary>
        public Race? MakerRace { get; }


        /// <summary>
        /// タグ情報
        /// </summary>
        public IReadOnlyList<string> Tags { get; }
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id"></param>
        protected Equipment(string id) : base(id)
        {
            var keyObj = new { EquipmentID = id };

            const string sql = "SELECT MacroName, EquipmentTypeID, Hull, HullIntegrated, Mk, MakerRace FROM Equipment WHERE EquipmentID = :EquipmentID";

            string equipmentTypeID;
            string? makerRace;
            (
                Macro,
                equipmentTypeID,
                Hull,
                HullIntegrated,
                Mk,
                makerRace
            ) = X4Database.Instance.QuerySingle<(string, string, long, bool, long, string?)>(sql, keyObj);

            EquipmentType = EquipmentType.Get(equipmentTypeID);

            MakerRace = (makerRace is not null) ? Race.Get(makerRace) : null;

            const string tagsSql = "SELECT Tag FROM EquipmentTag WHERE EquipmentID = :EquipmentID";
            Tags = X4Database.Instance.Query<string>(tagsSql, keyObj).ToArray();
        }



        /// <summary>
        /// インスタンスを作成
        /// </summary>
        /// <param name="id">装備品ID</param>
        /// <returns>装備品IDに対応するインスタンス</returns>
        public static Equipment? Create(string id)
        {
            const string sql = "SELECT EquipmentTypeID FROM Equipment WHERE EquipmentID = :EquipmentID UNION ALL SELECT '' AS EquipmentTypeID LIMIT 1";

            var equipmentTypeID = X4Database.Instance.QuerySingle<string>(sql, new { EquipmentID = id });
            if (string.IsNullOrEmpty(equipmentTypeID))
            {
                return null;
            }

            try
            {
                return equipmentTypeID switch
                {
                    "engines" => new Engine(id),
                    "shields" => new Shield(id),
                    "thrusters" => new Thruster(id),
                    _ => new Equipment(id)
                };
            }
            catch
            {
                return new Equipment(id);
            }
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is Equipment tgt && ID == tgt.ID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(ID);
    }
}

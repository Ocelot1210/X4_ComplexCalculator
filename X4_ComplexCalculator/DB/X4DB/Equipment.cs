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
        public HashSet<string> EquipmentTags { get; }


        /// <summary>
        /// サイズ
        /// </summary>
        public X4Size? Size { get; }
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">ウェアID</param>
        /// <param name="tags">タグ文字列</param>
        protected Equipment(string id, string tags) : base(id, tags)
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
            EquipmentTags = new HashSet<string>(X4Database.Instance.Query<string>(tagsSql, keyObj));

            Size = EquipmentTags.Select(x => X4Size.TryGet(x)).FirstOrDefault(x => x is not null);
        }



        /// <summary>
        /// インスタンスを作成
        /// </summary>
        /// <param name="id">ウェアID</param>
        /// <param name="tags">タグ文字列</param>
        /// <returns>装備品IDに対応するインスタンス</returns>
        public static Equipment? Create(string id, string tags)
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
                    "engines" => new Engine(id, tags),
                    "shields" => new Shield(id, tags),
                    "thrusters" => new Thruster(id, tags),
                    _ => new Equipment(id, tags)
                };
            }
            catch
            {
                return new Equipment(id, tags);
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

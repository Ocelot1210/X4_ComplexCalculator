using System;
using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 装備品管理用クラス
    /// </summary>
    public class Equipment
    {
        #region スタティックメンバ
        /// <summary>
        /// 装備一覧
        /// </summary>
        private readonly static Dictionary<string, Equipment> _Equipments = new();
        #endregion


        #region プロパティ
        /// <summary>
        /// 装備品ID
        /// </summary>
        public string EquipmentID { get; }


        /// <summary>
        /// マクロ名
        /// </summary>
        public string Macro { get; }


        /// <summary>
        /// 装備種別
        /// </summary>
        public EquipmentType EquipmentType { get; }


        /// <summary>
        /// 装備の大きさ
        /// </summary>
        public X4Size Size { get; }


        /// <summary>
        /// 装備名称
        /// </summary>
        public string Name { get; }


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
        /// 説明文
        /// </summary>
        public string Description { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentID">装備ID</param>
        /// <param name="macro">マクロ名</param>
        /// <param name="name">装備名称</param>
        /// <param name="equipmentTypeID">装備種別ID</param>
        /// <param name="sizeID">装備サイズ</param>
        /// <param name="hull">船体値</param>
        /// <param name="hullIntegrated">船体値が統合されているか</param>
        /// <param name="mk">MK</param>
        /// <param name="makerRace">製造種族</param>
        /// <param name="description">説明文</param>
        protected Equipment(
            string equipmentID,
            string macro,
            string name,
            string equipmentTypeID,
            string sizeID,
            long hull,
            bool hullIntegrated,
            long mk,
            string? makerRace,
            string description
        )
        {
            EquipmentID = equipmentID;
            Macro = macro;
            Name = name;
            EquipmentType = EquipmentType.Get(equipmentTypeID);
            Size = X4Size.Get(sizeID);
            Hull = hull;
            HullIntegrated = hullIntegrated;
            Mk = mk;
            MakerRace = (makerRace is not null) ? Race.Get(makerRace) : null;
            Description = description;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _Equipments.Clear();

            const string sql1 = "SELECT EquipmentID FROM Equipment";
            const string sql2 = "SELECT MacroName, EquipmentTypeID, SizeID, Name, Hull, HullIntegrated, Mk, MakerRace, Description FROM Equipment WHERE EquipmentID = :EquipmentID";

            foreach (var equipmentID in X4Database.Instance.Query<string>(sql1))
            {
                var (macro, equipmentTypeID, sizeID, name, hull, hullIntegrated, mk, makerRace, description) = 
                    X4Database.Instance.QuerySingle<(string, string, string, string, long, bool, long, string?, string)>
                    (sql2, new { EquipmentID = equipmentID });

                var entity = equipmentTypeID switch
                {
                    "engines" => new Engine(equipmentID, macro, name, equipmentTypeID, sizeID, hull, hullIntegrated, mk, makerRace, description),
                    "shields" => new Shield(equipmentID, macro, name, equipmentTypeID, sizeID, hull, hullIntegrated, mk, makerRace, description),
                    _ => new Equipment(equipmentID, macro, name, equipmentTypeID, sizeID, hull, hullIntegrated, mk, makerRace, description)
                };

                _Equipments.Add(equipmentID, entity);
            }
        }



        /// <summary>
        /// 装備IDに対応する装備を取得する
        /// </summary>
        /// <param name="equipmentID">装備ID</param>
        /// <returns>装備</returns>
        public static Equipment? Get(string equipmentID) =>
            _Equipments.TryGetValue(equipmentID, out var equipment) ? equipment : null;


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is Equipment tgt && EquipmentID == tgt.EquipmentID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(EquipmentID);
    }
}

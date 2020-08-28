using System;
using System.Collections.Generic;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュールの装備管理用クラス
    /// </summary>
    public class ModuleEquipment
    {
        #region スタティックメンバ
        /// <summary>
        /// モジュールの装備一覧(空の装備)
        /// </summary>
        private readonly static Dictionary<string, ModuleEquipment> _ModuleEquipments = new Dictionary<string, ModuleEquipment>();
        #endregion


        #region "プロパティ"
        /// <summary>
        /// タレット情報
        /// </summary>
        public ModuleEquipmentManager Turret { get; }


        /// <summary>
        /// シールド情報
        /// </summary>
        public ModuleEquipmentManager Shield { get; }


        /// <summary>
        /// 装備を持っているか
        /// </summary>
        public bool CanEquipped => Turret.CanEquipped | Shield.CanEquipped;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        private ModuleEquipment(string moduleID)
        {
            Turret = new ModuleEquipmentManager(moduleID, "Turret");
            Shield = new ModuleEquipmentManager(moduleID, "Shield");
        }


        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="moduleEquipment"></param>
        private ModuleEquipment(ModuleEquipment moduleEquipment)
        {
            Turret = new ModuleEquipmentManager(moduleEquipment.Turret);
            Shield = new ModuleEquipmentManager(moduleEquipment.Shield);
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _ModuleEquipments.Clear();

            DBConnection.X4DB.ExecQuery($"SELECT ModuleID FROM Module", (dr, args) =>
            {
                var id = (string)dr["ModuleID"];
                _ModuleEquipments.Add(id, new ModuleEquipment(id));
            });
        }


        /// <summary>
        /// モジュールIDに対応する空のモジュール装備を取得する
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <returns>空のモジュール装備</returns>
        public static ModuleEquipment Get(string moduleID)
        {
            var ret = _ModuleEquipments[moduleID] ?? throw new ArgumentException();

            // 装備不能なモジュールの場合、インスタンスを使い回す
            return ret.CanEquipped ? new ModuleEquipment(ret) : ret;
        }


        /// <summary>
        /// 全装備を列挙する
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Equipment> GetAllEquipment() => Turret.AllEquipments.Concat(Shield.AllEquipments);


        /// <summary>
        /// オブジェクトが同一か判定
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return obj is ModuleEquipment equipment &&
                   EqualityComparer<ModuleEquipmentManager>.Default.Equals(Turret, equipment.Turret) &&
                   EqualityComparer<ModuleEquipmentManager>.Default.Equals(Shield, equipment.Shield);
        }


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(Turret, Shield);
    }
}

using System;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュールの装備管理用クラス
    /// </summary>
    public class ModuleEquipment
    {
        #region "プロパティ"
        /// <summary>
        /// タレット情報
        /// </summary>
        public ModuleEquipmentManager Turret { get; set; }


        /// <summary>
        /// シールド情報
        /// </summary>
        public ModuleEquipmentManager Shield { get; set; }


        /// <summary>
        /// 装備を持っているか
        /// </summary>
        public bool CanEquipped => Turret.CanEquipped | Shield.CanEquipped;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        public ModuleEquipment(string moduleID)
        {
            Turret = new ModuleEquipmentManager(moduleID, "Turret");
            Shield = new ModuleEquipmentManager(moduleID, "Shield");
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var tgt = obj as ModuleEquipment;
            if (tgt == null) return false;

            return Turret.Equals(tgt.Turret) && Shield.Equals(tgt.Shield);
        }


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode()
        {
            return Turret.GetHashCode() ^ Shield.GetHashCode();
        }
    }
}

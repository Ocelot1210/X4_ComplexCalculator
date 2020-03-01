using System;
using System.Data.SQLite;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュール情報管理用クラス
    /// </summary>
    public class Module : IComparable
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ModuleID { get; private set; }


        /// <summary>
        /// モジュール種別
        /// </summary>
        public ModuleType ModuleType { get; private set; }


        /// <summary>
        /// モジュール名称
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// 従業員
        /// </summary>
        public long MaxWorkers { get; private set; }


        /// <summary>
        /// 最大収容人数
        /// </summary>
        public long WorkersCapacity { get; private set; }


        /// <summary>
        /// 装備情報
        /// </summary>
        public ModuleEquipment Equipment { get; set; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        public Module(string moduleID)
        {
            ModuleID = moduleID;

            DBConnection.X4DB.ExecQuery($"SELECT ModuleTypeID, Name, MaxWorkers, WorkersCapacity FROM Module WHERE Module.ModuleID = '{moduleID}'",
                (SQLiteDataReader dr, object[] args) =>
                {
                    Name = dr["Name"].ToString();
                    ModuleType = new ModuleType(dr["ModuleTypeID"].ToString());
                    MaxWorkers = (long)dr["MaxWorkers"];
                    WorkersCapacity = (long)dr["WorkersCapacity"];
                });

            Equipment = new ModuleEquipment(moduleID);
        }


        /// <summary>
        /// 文字列化
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }


        /// <summary>
        /// オブジェクト比較
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            var tgt = obj as Module;
            if (tgt == null) return 1;

            return ModuleID.CompareTo(tgt.ModuleID);
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var tgt = obj as Module;
            if (tgt == null) return false;

            return ModuleID == tgt.ModuleID && Equipment == tgt.Equipment;
        }


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode()
        {
            return ModuleID.GetHashCode();
        }
    }
}

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
        public string ModuleID { get; }


        /// <summary>
        /// モジュール種別
        /// </summary>
        public ModuleType ModuleType { get; }


        /// <summary>
        /// モジュール名称
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 従業員
        /// </summary>
        public long MaxWorkers { get; }


        /// <summary>
        /// 最大収容人数
        /// </summary>
        public long WorkersCapacity { get; }


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
            string name = "";
            long maxWorkers = 0;
            long workersCapacity = 0;
            ModuleType moduleType = null;

            DBConnection.X4DB.ExecQuery($"SELECT ModuleTypeID, Name, MaxWorkers, WorkersCapacity FROM Module WHERE Module.ModuleID = '{moduleID}'",
                (SQLiteDataReader dr, object[] args) =>
                {
                    name            = dr["Name"].ToString();
                    moduleType      = new ModuleType(dr["ModuleTypeID"].ToString());
                    maxWorkers      = (long)dr["MaxWorkers"];
                    workersCapacity = (long)dr["WorkersCapacity"];
                });

            Name = name;
            ModuleType = moduleType;
            MaxWorkers = maxWorkers;
            WorkersCapacity = workersCapacity;
            Equipment = new ModuleEquipment(moduleID);
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="module"></param>
        public Module(Module module)
        {
            ModuleID        = module.ModuleID;
            Name            = module.Name;
            ModuleType      = new ModuleType(module.ModuleType);
            MaxWorkers      = module.MaxWorkers;
            WorkersCapacity = module.WorkersCapacity;
            Equipment       = new ModuleEquipment(module.Equipment);
        }

        /// <summary>
        /// 装備を追加
        /// </summary>
        /// <param name="equipment">追加したい装備</param>
        public void AddEquipment(Equipment equipment)
        {
            // 装備できないモジュールの場合、何もしない
            if (!Equipment.CanEquipped)
            {
                return;
            }

            switch (equipment.EquipmentType.EquipmentTypeID)
            {
                case "turrets":
                    Equipment.Turret.AddEquipment(equipment);
                    break;

                case "shields":
                    Equipment.Shield.AddEquipment(equipment);
                    break;

                default:
                    throw new InvalidOperationException("追加できるのはタレットかシールドのみです。");
            }
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

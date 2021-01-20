using System;
using System.Collections.Generic;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュール情報管理用クラス
    /// </summary>
    public class Module : Ware, IComparable
    {
        #region スタティックメンバ
        /// <summary>
        /// モジュール一覧
        /// </summary>
        private readonly static Dictionary<string, Module> _Modules = new();
        #endregion


        #region プロパティ
        /// <summary>
        /// モジュール種別
        /// </summary>
        public ModuleType ModuleType { get; }


        /// <summary>
        /// 従業員
        /// </summary>
        public long MaxWorkers { get; }


        /// <summary>
        /// 最大収容人数
        /// </summary>
        public long WorkersCapacity { get; }


        /// <summary>
        /// 設計図が無いか
        /// </summary>
        public bool NoBluePrint { get; }


        /// <summary>
        /// モジュールの製品
        /// </summary>
        public IReadOnlyList<ModuleProduct> Product { get; }


        /// <summary>
        /// 装備可能なタレットの数
        /// </summary>
        public IReadOnlyDictionary<X4Size, int> TurretCapacity { get; }


        /// <summary>
        /// 装備可能なシールドの数
        /// </summary>
        public IReadOnlyDictionary<X4Size, int> ShieldCapacity { get; }


        /// <summary>
        /// 保管庫情報
        /// </summary>
        public ModuleStorage Storage { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">ウェアID</param>
        /// <param name="tags">タグ文字列</param>
        public Module(string id, string tags) : base(id, tags)
        {
            string moduleTypeID;
            var keyObj = new { ID = id };

            const string sql1 = "SELECT ModuleTypeID, MaxWorkers, WorkersCapacity, NoBluePrint FROM Module WHERE ModuleID = :ID";
            (
                moduleTypeID,
                MaxWorkers,
                WorkersCapacity,
                NoBluePrint
            ) = X4Database.Instance.QuerySingle<(string, long, long, bool)>(sql1, keyObj);


            TurretCapacity = Equipments.Values
                .Where(x => x.EquipmentType.EquipmentTypeID == "turrets")
                .SelectMany(x => x.Tags.Select(y => X4Size.TryGet(y)))
                .Where(x => x is not null)
                .Select(x => x!)
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());

            ShieldCapacity = Equipments.Values
                .Where(x => x.EquipmentType.EquipmentTypeID == "shields")
                .SelectMany(x => x.Tags.Select(y => X4Size.TryGet(y)))
                .Where(x => x is not null)
                .Select(x => x!)
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());

            ModuleType = ModuleType.Get(moduleTypeID);

            Product = ModuleProduct.Get(id);

            Storage = ModuleStorage.Get(id);
        }



        /// <summary>
        /// 全モジュール情報を取得する
        /// </summary>
        public static new IEnumerable<Module> GetAll() => _Modules.Values;


        /// <summary>
        /// オブジェクト比較
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object? obj)
        {
            if (obj is not Module tgt)
            {
                return 1;
            }

            return ID.CompareTo(tgt.ID);
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is Module tgt && ID == tgt.ID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(ID);


        /// <summary>
        /// X4 データベース内の Module テーブルのレコードを表す構造体
        /// </summary>
        private class ModuleTable
        {
            public string ModuleID { get; }
            public string ModuleTypeID { get; }
            public string Name { get; }
            public long MaxWorkers { get; }
            public long WorkersCapacity { get; }
            public bool NoBlueprint { get; }

            public ModuleTable(string moduleID, string moduleTypeID, string name,
                               long maxWorkers, long workersCapacity, bool noBlueprint)
            {
                ModuleID = moduleID;
                Name = name;
                ModuleTypeID = moduleTypeID;
                MaxWorkers = maxWorkers;
                WorkersCapacity = workersCapacity;
                NoBlueprint = noBlueprint;
            }
        }
    }
}

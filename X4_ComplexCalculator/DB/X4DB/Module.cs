using System;
using System.Collections.Generic;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュール情報管理用クラス
    /// </summary>
    public class Module : IComparable
    {
        #region スタティックメンバ
        /// <summary>
        /// モジュール一覧
        /// </summary>
        private readonly static Dictionary<string, Module> _Modules = new();
        #endregion


        #region プロパティ
        /// <summary>
        /// モジュールID
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
        /// 建造方式
        /// </summary>
        public IReadOnlyList<ModuleProduction> ModuleProductions { get; }


        /// <summary>
        /// 所有派閥
        /// </summary>
        public IReadOnlyList<Faction> Owners { get; }


        /// <summary>
        /// 装備可能なタレットの数
        /// </summary>
        public IReadOnlyDictionary<X4Size, int> TurretCapacity { get; }


        /// <summary>
        /// 装備可能なシールドの数
        /// </summary>
        public IReadOnlyDictionary<X4Size, int> ShieldCapacity { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="name">モジュール名</param>
        /// <param name="moduleType">モジュール種別</param>
        /// <param name="maxWorkers">従業員数</param>
        /// <param name="workersCapacity">従業員収容人数</param>
        /// <param name="buildMethods">建造方式</param>
        /// <param name="owners">所有派閥</param>
        /// <param name="turretCapacity">装備可能なタレットの数</param>
        /// <param name="shieldCapacity">装備可能なシールドの数</param>
        private Module(string moduleID, string name, ModuleType moduleType,
                       long maxWorkers, long workersCapacity,
                       ModuleProduction[] buildMethods, Faction[] owners,
                       Dictionary<X4Size, int> turretCapacity,
                       Dictionary<X4Size, int> shieldCapacity)
        {
            ModuleID = moduleID;
            Name = name;
            ModuleType = moduleType;
            MaxWorkers = maxWorkers;
            WorkersCapacity = workersCapacity;
            Owners = owners;
            ModuleProductions = buildMethods;
            TurretCapacity = turretCapacity;
            ShieldCapacity = shieldCapacity;
        }



        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _Modules.Clear();

            const string sql1 = "SELECT ModuleID, ModuleTypeID, Name, MaxWorkers, WorkersCapacity, NoBlueprint FROM Module";
            foreach (var record in X4Database.Instance.Query<ModuleTable>(sql1))
            {
                var moduleType = ModuleType.Get(record.ModuleTypeID);

                const string sql2 = "SELECT FactionID FROM ModuleOwner WHERE ModuleID = :ModuleID";
                var owners = X4Database.Instance.Query<string>(sql2, record)
                    .Select<string, Faction>(Faction.Get!)
                    .Where(x => x != null)
                    .ToArray();

                var buildMethods = record.NoBlueprint
                    ? Array.Empty<ModuleProduction>()
                    : ModuleProduction.Get(record.ModuleID);

                const string sql3 = "SELECT SizeID, Amount FROM ModuleTurret WHERE ModuleID = :ModuleID";
                var turretCapacity = X4Database.Instance
                    .Query<(string sizeID, int amount)>(sql3, record)
                    .ToDictionary(t => X4Size.Get(t.sizeID), t => t.amount);

                const string sql4 = "SELECT SizeID, Amount FROM ModuleShield WHERE ModuleID = :ModuleID";
                var shieldCapacity = X4Database.Instance
                    .Query<(string sizeID, int amount)>(sql4, record)
                    .ToDictionary(t => X4Size.Get(t.sizeID), t => t.amount);

                var module = new Module(record.ModuleID, record.Name, moduleType,
                                        record.MaxWorkers, record.WorkersCapacity,
                                        buildMethods, owners, turretCapacity, shieldCapacity);
                _Modules.Add(record.ModuleID, module);
            }
        }


        /// <summary>
        /// モジュールIDに対応するモジュールを取得
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <returns>モジュール</returns>
        public static Module? Get(string moduleID) =>
            _Modules.TryGetValue(moduleID, out var module) ? module : null;


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

            return ModuleID.CompareTo(tgt.ModuleID);
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is Module tgt && ModuleID == tgt.ModuleID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(ModuleID);


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

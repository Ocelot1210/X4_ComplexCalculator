using System;
using System.Collections.Generic;


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
        private readonly static Dictionary<string, Module> _Modules = new Dictionary<string, Module>();
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
        public ModuleProduction[] ModuleProductions { get; }


        /// <summary>
        /// 所有派閥
        /// </summary>
        public Faction[] Owners { get; }
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
        private Module(string moduleID, string name, ModuleType moduleType, long maxWorkers, long workersCapacity, ModuleProduction[] buildMethods, Faction[] owners)
        {
            ModuleID = moduleID;
            Name = name;
            ModuleType = moduleType;
            MaxWorkers = maxWorkers;
            WorkersCapacity = workersCapacity;
            Owners = owners;
            ModuleProductions = buildMethods;
        }



        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _Modules.Clear();
            DBConnection.X4DB.ExecQuery("SELECT ModuleID, ModuleTypeID, Name, MaxWorkers, WorkersCapacity, NoBlueprint FROM Module", (dr, args) =>
            {
                var id = (string)dr["ModuleID"];
                var name = (string)dr["Name"];
                var moduleType = ModuleType.Get((string)dr["ModuleTypeID"]);
                var maxWorkers = (long)dr["MaxWorkers"];
                var workersCapacity = (long)dr["WorkersCapacity"];
                var noBlueprint = (long)dr["NoBlueprint"] == 1;

                var moduleOwners = new List<Faction>();
                DBConnection.X4DB.ExecQuery($"SELECT FactionID FROM ModuleOwner WHERE ModuleID = '{id}'", (dr2, args2) =>
                {
                    var faction = Faction.Get((string)dr2["FactionID"]);
                    if (faction != null) moduleOwners.Add(faction);
                });

                var prod = (noBlueprint) ? Array.Empty<ModuleProduction>() : ModuleProduction.Get(id);

                _Modules.Add(id, new Module(id, name, moduleType, maxWorkers, workersCapacity, prod, moduleOwners.ToArray()));
            });
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
            if (!(obj is Module tgt))
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
    }
}

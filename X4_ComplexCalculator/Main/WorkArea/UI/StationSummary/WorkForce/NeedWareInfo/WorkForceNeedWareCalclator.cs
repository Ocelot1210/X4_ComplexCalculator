using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.WorkForce.NeedWareInfo
{
    /// <summary>
    /// 労働者が必要とするウェア計算用クラス
    /// </summary>
    class WorkForceNeedWareCalclator
    {
        #region スタティックメンバ
        /// <summary>
        /// 労働者が必要とするウェア計算用シングルトンインスタンス
        /// </summary>
        private static WorkForceNeedWareCalclator? _SingletonInstance;
        #endregion


        #region メンバ
        /// <summary>
        /// 労働者が必要とするウェア一覧
        /// &lt;方式, &lt;(ウェアID, 個数)&gt;&gt;
        /// </summary>
        private IReadOnlyDictionary<string, (string, double)[]> _NeedWares;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        private WorkForceNeedWareCalclator()
        {
            var dict = new Dictionary<string, List<(string, double)>>();

            var query = @"
SELECT
	WorkUnitResource.Method,
	WorkUnitResource.WareID,
	CAST(WorkUnitResource.Amount AS REAL) / WorkUnitProduction.Amount AS Amount
	
FROM
	WorkUnitProduction,
	WorkUnitResource
	
WHERE
	WorkUnitResource.WorkUnitID = 'workunit_busy' AND
	WorkUnitResource.WorkUnitID = WorkUnitProduction.WorkUnitID AND
	WorkUnitResource.Method     = WorkUnitProduction.Method";

            DBConnection.X4DB.ExecQuery(query, (dr, _) =>
            {
                var method = (string)dr["Method"];
                var wareID = (string)dr["WareID"];
                var amount = (double)dr["Amount"];

                if (!dict.ContainsKey(method))
                {
                    dict.Add(method, new List<(string, double)>());
                }

                dict[method].Add((wareID, amount));
            });


            _NeedWares = dict.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }


        /// <summary>
        /// インスタンスを取得
        /// </summary>
        public static WorkForceNeedWareCalclator Instance
        {
            get
            {
                if (_SingletonInstance == null)
                {
                    _SingletonInstance = new WorkForceNeedWareCalclator();
                }

                return _SingletonInstance;
            }
        }


        /// <summary>
        /// 計算する
        /// </summary>
        /// <param name="modules">モジュール一覧</param>
        public Dictionary<string, (string WareID, long Amount)[]> Calc(IEnumerable<Module> modules)
        {
            var ret = new Dictionary<string, List<(string WareID, long Amount)>>();

            foreach (var module in modules)
            {
                var method = module.Owners.First().Race.RaceID;

                if (!_NeedWares.ContainsKey(method))
                {
                    method = "default";
                }

                var wares = _NeedWares[method];
                if (!ret.ContainsKey(method))
                {
                    ret.Add(method, new List<(string WareID, long Amount)>());
                }

                ret[method].AddRange(wares.Select(x => (x.Item1, (long)Math.Ceiling(x.Item2 * module.WorkersCapacity))));
            }

            return ret.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }


        /// <summary>
        /// 計算する
        /// </summary>
        /// <param name="modules">モジュール一覧</param>
        public Dictionary<string, (string WareID, long Amount)[]> Calc(IEnumerable<ModulesGridItem> modules)
        {
            var ret = new Dictionary<string, List<(string WareID, long Amount)>>();

            foreach (var module in modules)
            {
                var method = module.Module.Owners.First().Race.RaceID;

                if (!_NeedWares.ContainsKey(method))
                {
                    method = "default";
                }

                var wares = _NeedWares[method];
                if (!ret.ContainsKey(method))
                {
                    ret.Add(method, new List<(string WareID, long Amount)>());
                }

                ret[method].AddRange(wares.Select(x => (x.Item1, (long)Math.Ceiling(x.Item2 * module.Module.WorkersCapacity) * module.ModuleCount)));
            }

            return ret.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }
    }
}

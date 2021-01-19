﻿using System;
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
    class WorkForceNeedWareCalculator
    {
        #region スタティックメンバ
        /// <summary>
        /// 労働者が必要とするウェア計算用シングルトンインスタンス
        /// </summary>
        private static WorkForceNeedWareCalculator? _SingletonInstance;
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
        private WorkForceNeedWareCalculator()
        {
            var workUnit = Ware.Get("workunit_busy");
            _NeedWares = workUnit.Productions
                .ToDictionary(
                    x => x.Method,
                    x => workUnit.Resources[x.Method]
                    .Select(y => (y.NeedWareID, (double)y.Amount / x.Amount / (x.Time / 3600.0)))
                    .ToArray()
                );
        }


        /// <summary>
        /// インスタンスを取得
        /// </summary>
        public static WorkForceNeedWareCalculator Instance
        {
            get
            {
                if (_SingletonInstance is null)
                {
                    _SingletonInstance = new WorkForceNeedWareCalculator();
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
        public Dictionary<string, IReadOnlyList<(string WareID, long Amount)>> Calc(IEnumerable<ModulesGridItem> modules)
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

            return ret.ToDictionary(x => x.Key, x => x.Value as IReadOnlyList<(string, long)>);
        }
    }
}

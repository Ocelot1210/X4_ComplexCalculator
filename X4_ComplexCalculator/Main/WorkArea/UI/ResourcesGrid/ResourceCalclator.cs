using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ResourcesGrid
{
    /// <summary>
    /// 建造リソース計算用クラス
    /// </summary>
    class ResourceCalclator
    {
        #region スタティックメンバ
        /// <summary>
        /// 建造リソース計算用シングルトンインスタンス
        /// </summary>
        private static ResourceCalclator? _SingletonInstance;
        #endregion


        #region メンバ
        /// <summary>
        /// 建造リソース一覧
        /// &lt;ModuleID, &lt;Method, (WareID, Amount)&gt;&gt;
        /// </summary>
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, (string, long)[]>> _BuildResource;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        private ResourceCalclator()
        {
            var resources = new Dictionary<string, Dictionary<string, List<(string, long)>>>();

            // モジュールの生産に必要なリソース一覧を取得
            {
                DBConnection.X4DB.ExecQuery("SELECT ModuleID, Method, WareID, Amount FROM ModuleResource", (dr, _) =>
                {
                    var id = (string)dr["ModuleID"];
                    if (!resources.ContainsKey(id))
                    {
                        resources.Add(id, new Dictionary<string, List<(string, long)>>());
                    }

                    var method = (string)dr["Method"];
                    if (!resources[id].ContainsKey(method))
                    {
                        resources[id].Add(method, new List<(string, long)>());
                    }

                    var wareID = (string)dr["WareID"];
                    var amount = (long)dr["Amount"];
                    resources[id][method].Add((wareID, amount));
                });
            }


            // 装備の生産に必要なリソース一覧を取得
            {
                var query = @"
SELECT
    EquipmentResource.EquipmentID,
    EquipmentResource.Method,
    EquipmentResource.NeedWareID,
    EquipmentResource.Amount
FROM
    Equipment,
    EquipmentResource

WHERE
    Equipment.EquipmentID = EquipmentResource.EquipmentID AND
    Equipment.EquipmentTypeID IN ('turrets', 'shields')";

                DBConnection.X4DB.ExecQuery(query, (dr, _) =>
                {
                    var id = (string)dr["EquipmentID"];
                    if (!resources.ContainsKey(id))
                    {
                        resources.Add(id, new Dictionary<string, List<(string, long)>>());
                    }

                    var method = (string)dr["Method"];
                    if (!resources[id].ContainsKey(method))
                    {
                        resources[id].Add(method, new List<(string, long)>());
                    }

                    var wareID = (string)dr["NeedWareID"];
                    var amount = (long)dr["Amount"];
                    resources[id][method].Add((wareID, amount));
                });
            }

            _BuildResource = resources.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Key, y => y.Value.ToArray()) as IReadOnlyDictionary<string, (string, long)[]>);
        }


        /// <summary>
        /// 建造リソース計算用クラスのインスタンス
        /// </summary>
        public static ResourceCalclator Instance
        {
            get
            {
                // 未作成なら作成する
                if (_SingletonInstance == null)
                {
                    _SingletonInstance = new ResourceCalclator();
                }

                return _SingletonInstance;
            }
        }


        /// <summary>
        /// 建造に必要なウェアを計算
        /// </summary>
        /// <param name="Items">モジュール/装備ID, 建造方式, ジュール/装備数</param>
        /// <returns></returns>
        public Dictionary<string, long> CalcResource(IEnumerable<(string ID, string Method, long Count)> Items)
        {
            var ret = new Dictionary<string, long>();

            foreach (var (id, method, count) in Items)
            {
                foreach (var (wareID, amount) in CalcResource(id, method))
                {
                    if (!ret.ContainsKey(wareID))
                    {
                        ret.Add(wareID, amount * count);
                    }
                    else
                    {
                        ret[wareID] += amount * count;
                    }
                }
            }

            return ret;
        }


        /// <summary>
        /// 建造に必要なウェアを計算
        /// </summary>
        /// <param name="id">モジュール/装備ID</param>
        /// <returns>建造に必要なウェアと個数</returns>
        public IEnumerable<(string WareID, long Amount)> CalcResource(string id, string method)
        {
            var kvp = _BuildResource[id] ?? throw new InvalidOperationException();

            var b = kvp[method];
            if (!b.Any())
            {
                b = kvp["default"];
            }

            return b;
        }
    }
}

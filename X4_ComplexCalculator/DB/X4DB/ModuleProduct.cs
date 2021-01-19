using System;
using System.Collections.Generic;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュールの製品情報管理クラス
    /// </summary>
    public class ModuleProduct
    {
        #region スタティックメンバ
        /// <summary>
        /// モジュールの製品情報一覧
        /// </summary>
        private static readonly Dictionary<string, IReadOnlyList<ModuleProduct>> _ModuleProducts = new();

        /// <summary>
        /// ダミーの製品情報一覧
        /// </summary>
        private static readonly IReadOnlyList<ModuleProduct> _DummyProduct = Array.Empty<ModuleProduct>();
        #endregion


        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ModuleID { get; }


        /// <summary>
        /// 生産対象のウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// 製造方式
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// 生産情報
        /// </summary>
        public WareProduction WareProduction { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="wareID">ウェアID</param>
        /// <param name="method">製造方式</param>
        private ModuleProduct(string moduleID, string wareID, string method)
        {
            ModuleID = moduleID;
            WareID = wareID;
            Method = method;

            var prod = WareProduction.Get(WareID, method);
            if (prod is null)
            {
                // ウェアIDと生産方式に対応する生産情報が存在しない
                throw new Exception($@"There is no ""ModuleProduction"" corresponding to ""WareID"" And ""Methood""\r\nWareID : {WareID}\r\nMethod : {Method}");
            }

            WareProduction = prod;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _ModuleProducts.Clear();

            const string sql = "SELECT ModuleID, WareID, Method FROM ModuleProduct";
            foreach (var item in X4Database.Instance.Query<ModuleProduct>(sql).GroupBy(x => x.ModuleID))
            {
                _ModuleProducts.Add(item.Key, item.ToArray());
            }
        }


        /// <summary>
        /// モジュールIDに対応するモジュールの製品情報を取得する
        /// </summary>
        /// <param name="moduleID>モジュールID</param>
        /// <returns>モジュールの製品情報</returns>
        public static IReadOnlyList<ModuleProduct> Get(string moduleID) => 
            _ModuleProducts.TryGetValue(moduleID, out var ret) ? ret : _DummyProduct;
    }
}

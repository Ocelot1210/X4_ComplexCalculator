using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid
{
    /// <summary>
    /// 製品計算用クラス
    /// </summary>
    class ProductCalculator
    {
        #region スタティックメンバ
        /// <summary>
        /// 製品計算用シングルトンインスタンス
        /// </summary>
        private static ProductCalculator? _SingletonInstance;
        #endregion


        #region メンバ
        /// <summary>
        /// ウェアとウェアを生産するモジュールを対応付けたディクショナリ
        /// </summary>
        private readonly Dictionary<Ware, Module> _Ware2ModuleDict;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        private ProductCalculator()
        {
            // ウェアとウェアを生産するモジュールを対応付けたディクショナリを初期化
            _Ware2ModuleDict = Ware.GetAll<Module>()
                .Where(x => x.Product.Any())
                .GroupBy(x => Ware.Get((x.Product.FirstOrDefault(y => y.Method == "default") ?? x.Product.First()).WareID))
                .ToDictionary(x => x.Key, x => x.First());
        }


        /// <summary>
        /// 製品計算用クラスのインスタンス
        /// </summary>
        public static ProductCalculator Instance
        {
            get
            {
                // 未作成なら作成する
                if (_SingletonInstance is null)
                {
                    _SingletonInstance = new ProductCalculator();
                }

                return _SingletonInstance;
            }
        }


        /// <summary>
        /// 製品と必要ウェアを計算
        /// </summary>
        /// <param name="module"></param>
        /// <param name="moduleCount">モジュール数</param>
        /// <returns></returns>
        public IEnumerable<CalcResult> Calc(Module module, long moduleCount)
        {
            return CalcProductAndResources(module, moduleCount)
                .Concat(CalcHabitationModuleResources(module, moduleCount));
        }


        /// <summary>
        /// 製造モジュールの製品と必要ウェアを計算
        /// </summary>
        /// <param name="module">計算対象のモジュール</param>
        /// <param name="moduleCount">モジュール数</param>
        /// <returns>製品と必要ウェアの列挙</returns>
        private IEnumerable<CalcResult> CalcProductAndResources(Module module, long moduleCount)
        {
            // モジュールの生産品を取得
            foreach (var product in module.Product)
            {
                // 生産品IDに対応するウェアを取得
                var prodWare = Ware.Get(product.WareID);

                // ウェア生産方式を取得
                var method = prodWare.Resources.ContainsKey(product.Method) ? product.Method : "default";


                // ウェア生産情報を取得
                var wareProduction = WareProduction.Get(product.WareID, product.Method) ?? throw new ArgumentException();
                {
                    // ウェア生産時の追加効果一覧をウェア生産方式別に抽出
                    var effects = WareEffect.Get(product.WareID, product.Method);

                    // ウェア生産量
                    var amount = (long)Math.Floor(wareProduction.Amount * (3600 / wareProduction.Time));

                    yield return new CalcResult(product.WareID, amount, method, module, moduleCount, effects);
                }


                // 有効なウェア生産方式か？
                if (prodWare.Resources.TryGetValue(method, out var resources))
                {
                    foreach (var resource in resources)
                    {
                        // ウェア消費量
                        var amount = (long)Math.Floor(-3600 / wareProduction.Time * resource.Amount);
                        yield return new CalcResult(resource.NeedWareID, amount, method, module, moduleCount);
                    }
                }
            }
        }


        /// <summary>
        /// 居住モジュールの必要ウェアを計算
        /// </summary>
        /// <param name="module">計算対象モジュール</param>
        /// <param name="moduleCount">モジュール数</param>
        /// <returns>必要ウェアの列挙</returns>
        private IEnumerable<CalcResult> CalcHabitationModuleResources(Module module, long moduleCount)
        {
            // 居住モジュールか？
            if (0 < module.WorkersCapacity)
            {
                // 居住モジュールの種族を取得
                var ownerRace = module.Owners.FirstOrDefault()?.Race;
                if (ownerRace is not null)
                {
                    var workUnit = Ware.Get("workunit_busy");

                    var method = workUnit.Resources.ContainsKey(ownerRace.RaceID) ? ownerRace.RaceID : "default";

                    var prod =
                        workUnit.Productions.FirstOrDefault(x => x.Method == method) ??
                        workUnit.Productions.FirstOrDefault(x => x.Method == "default") ??
                        throw new InvalidOperationException();

                    if (workUnit.Resources.TryGetValue(method, out var resources))
                    {
                        var xx = workUnit.Productions;

                        foreach (var resource in resources)
                        {
                            // ウェア消費量
                            var amount = (long)Math.Floor(-3600 / prod.Time * resource.Amount);
                            yield return new CalcResult(resource.NeedWareID, amount, method, module, moduleCount);
                        }
                    }
                }
            }
        }





        /// <summary>
        /// 必要モジュールを計算
        /// </summary>
        /// <param name="products">製品一覧</param>
        /// <param name="settings">ステーションの設定</param>
        /// <returns>必要モジュールと個数のタプル</returns>
        public IEnumerable<(Module Module, long Count)> CalcNeedModules(IReadOnlyList<ProductsGridItem> products, IStationSettings settings)
        {
            var addModules = new Dictionary<Module, long>();                        // 追加予定モジュールと個数のディクショナリ
            var addModuleProducts = new List<(Ware Ware, long Count)>();            // 追加予定モジュールの製品一覧
            var excludeWares = new HashSet<Ware>();                                 // 計算除外製品一覧

            foreach (var prod in products.Where(x => 0 < x.Ware.WareGroup.Tier).OrderBy(x => x.Ware.WareGroup.Tier))
            {
                // 追加予定のモジュールも含めた製造ウェア数
                var totalCount = prod.Count + addModuleProducts
                    .Where(x => x.Ware.Equals(prod.Ware))
                    .Sum(x => x.Count);

                // 不足していない or 計算除外ウェアの場合、何もしない
                if (0 <= totalCount || excludeWares.Contains(prod.Ware))
                {
                    continue;
                }


                // 不足しているウェアを製造するモジュールを取得
                if (_Ware2ModuleDict.TryGetValue(prod.Ware, out var module))
                {
                    var modCount = 0L;

                    // モジュールが製造するウェアを取得
                    foreach (var addProduct in CalcProductAndResources(module, 1))
                    {
                        var addAmount = addProduct.WareAmount;

                        if (addProduct.Efficiency is not null && addProduct.Efficiency.TryGetValue("sunlight", out var wareEffect))
                        {
                            addAmount = (long)Math.Floor(addAmount * wareEffect.Product * settings.Sunlight);
                        }

                        // 生産数が0の場合、計算除外製品一覧に突っ込む
                        if (addAmount == 0)
                        {
                            excludeWares.Add(prod.Ware);
                            continue;
                        }

                        // 製造するウェア数からあと何モジュール必要か計算する
                        modCount = Math.Max(modCount, (long)Math.Ceiling(-(double)totalCount / addAmount));

                        // 追加予定モジュールの製品一覧を更新
                        addModuleProducts.Add((Ware.Get(addProduct.WareID), addProduct.WareAmount * modCount));
                    }

                    // 追加予定モジュールにモジュールを追加
                    if (addModules.ContainsKey(module))
                    {
                        addModules[module] += modCount;
                    }
                    else
                    {
                        addModules.Add(module, modCount);
                    }
                }
            }

            return addModules.Select(x => (x.Key, x.Value));
        }
    }
}

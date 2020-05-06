using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.ResourcesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReader
{
    /// <summary>
    /// 基本となる保存ファイル読み込み
    /// </summary>
    class SaveDataReader0 : ISaveDataReader
    {
        /// <summary>
        /// 読み込み対象ファイルパス
        /// </summary>
        public string Path { set; protected get; }


        /// <summary>
        /// モジュール一覧
        /// </summary>
        protected readonly ObservableRangeCollection<ModulesGridItem> _Modules;

        /// <summary>
        /// 製品一覧
        /// </summary>
        protected readonly ObservableRangeCollection<ProductsGridItem> _Products;

        /// <summary>
        /// 建造リソース一覧
        /// </summary>
        protected readonly ObservableRangeCollection<ResourcesGridItem> _Resources;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modules">モジュール一覧</param>
        /// <param name="products">製品一覧</param>
        /// <param name="resources">建造リソース一覧</param>
        public SaveDataReader0(
            ObservableRangeCollection<ModulesGridItem> modules,
            ObservableRangeCollection<ProductsGridItem> products,
            ObservableRangeCollection<ResourcesGridItem> resources
        )
        {
            _Modules = modules;
            _Products = products;
            _Resources = resources;
        }


        /// <summary>
        /// ファイル読み込み
        /// </summary>
        /// <returns>成功したか</returns>
        public virtual bool Load()
        {
            var ret = false;

            using (var conn = new DBConnection(Path))
            {
                try
                {
                    conn.BeginTransaction();

                    // モジュール復元
                    RestoreModules(conn);

                    // 製品価格を復元
                    RestoreProductsPrice(conn);

                    // 建造リソースを復元
                    RestoreBuildResource(conn);

                    ret = true;
                }
                catch
                {

                }
                finally
                {
                    conn.Rollback();
                }
            }

            return ret;
        }


        /// <summary>
        /// モジュールを復元
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="args"></param>
        protected virtual void RestoreModules(DBConnection conn)
        {
            var moduleCnt = 0;

            conn.ExecQuery("SELECT count(*) AS Count from Modules", (dr, _) =>
            {
                moduleCnt = (int)(long)dr[0];
            });

            var modules = new List<ModulesGridItem>(moduleCnt);

            // モジュールを復元
            conn.ExecQuery("SELECT ModuleID, Count FROM Modules ORDER BY Row ASC", (dr, _) =>
            {
                modules.Add(new ModulesGridItem((string)dr["ModuleID"], (long)dr["Count"]));
            });

            // モジュールの装備を復元
            conn.ExecQuery($"SELECT * FROM Equipments", (dr, _) =>
            {
                modules[(int)(long)dr["row"]].Module.AddEquipment(new Equipment((string)dr["EquipmentID"]));
            });

            _Modules.Reset(modules);
        }


        /// <summary>
        /// 製品価格を復元
        /// </summary>
        /// <param name="conn"></param>
        protected virtual void RestoreProductsPrice(DBConnection conn)
        {
            conn.ExecQuery($"SELECT WareID, Price FROM Products", (dr, _) =>
            {
                var wareID = (string)dr["WareID"];
                var itm = _Products.Where(x => x.Ware.WareID == wareID).FirstOrDefault();
                if (itm != null)
                {
                    itm.UnitPrice = (long)dr["Price"];
                }
            });
        }


        /// <summary>
        /// 建造リソースを復元
        /// </summary>
        /// <param name="conn"></param>
        protected virtual void RestoreBuildResource(DBConnection conn)
        {
            conn.ExecQuery($"SELECT WareID, Price FROM BuildResources", (dr, _) =>
            {
                var wareID = (string)dr["WareID"];

                var itm = _Resources.Where(x => x.Ware.WareID == wareID).FirstOrDefault();
                if (itm != null)
                {
                    itm.UnitPrice = (long)dr["Price"];
                }
            });
        }
    }
}

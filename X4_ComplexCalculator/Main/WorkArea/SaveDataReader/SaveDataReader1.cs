using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.ResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.StorageAssign;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReader
{
    /// <summary>
    /// Ver1用セーブファイル読み込み用クラス
    /// </summary>
    class SaveDataReader1 : SaveDataReader0
    {
        /// <summary>
        /// 保管庫割当情報
        /// </summary>
        protected readonly ObservableRangeCollection<StorageAssignGridItem> _StorageAssignItems;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modules">モジュール一覧</param>
        /// <param name="products">製品一覧</param>
        /// <param name="resources">建造リソース一覧</param>
        /// <param name="storageAssignItems">保管庫割当情報</param>
        public SaveDataReader1(
            ObservableRangeCollection<ModulesGridItem> modules,
            ObservableRangeCollection<ProductsGridItem> products,
            ObservableRangeCollection<ResourcesGridItem> resources,
            ObservableRangeCollection<StorageAssignGridItem> storageAssignItems
        ) : base(modules, products, resources)
        {
            _StorageAssignItems = storageAssignItems;
        }


        /// <summary>
        /// ファイル読み込み
        /// </summary>
        /// <returns>成功したか</returns>
        public override bool Load()
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

                    //保管庫割当情報を読み込み
                    RestoreStorageAssignInfo(conn);

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
        /// 保管庫割当情報を読み込み
        /// </summary>
        /// <param name="conn"></param>
        protected virtual void RestoreStorageAssignInfo(DBConnection conn)
        {
            conn.ExecQuery($"SELECT WareID, AllocCount FROM StorageAssign", (dr, _) =>
            {
                var wareID = (string)dr["WareID"];

                var itm = _StorageAssignItems.Where(x => x.WareID == wareID).FirstOrDefault();
                if (itm != null)
                {
                    itm.AllocCount = (long)dr["AllocCount"];
                }
            });
        }
    }
}

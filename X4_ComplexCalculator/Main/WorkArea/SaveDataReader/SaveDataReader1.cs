using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReader
{
    /// <summary>
    /// Ver1用セーブファイル読み込み用クラス
    /// </summary>
    class SaveDataReader1 : SaveDataReader0
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="workArea">作業エリア</param>
        public SaveDataReader1(IWorkArea workArea) : base(workArea)
        {
            
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

                var itm = _WorkArea.StorageAssign.Where(x => x.WareID == wareID).FirstOrDefault();
                if (itm != null)
                {
                    itm.AllocCount = (long)dr["AllocCount"];
                }
            });
        }
    }
}

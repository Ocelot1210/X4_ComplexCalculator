using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

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
        public string Path { set; protected get; } = "";

        /// <summary>
        /// 作業エリア
        /// </summary>
        protected readonly IWorkArea _WorkArea;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="WorkArea">作業エリア</param>
        public SaveDataReader0(IWorkArea WorkArea)
        {
            _WorkArea = WorkArea;
        }


        /// <summary>
        /// ファイル読み込み
        /// </summary>
        /// <returns>成功したか</returns>
        public virtual bool Load(IProgress<int> progress)
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

                    _WorkArea.Title = System.IO.Path.GetFileNameWithoutExtension(Path);

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

            _WorkArea.Modules.Reset(modules);
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
                var itm = _WorkArea.Products.Where(x => x.Ware.WareID == wareID).FirstOrDefault();
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

                var itm = _WorkArea.Resources.Where(x => x.Ware.WareID == wareID).FirstOrDefault();
                if (itm != null)
                {
                    itm.UnitPrice = (long)dr["Price"];
                }
            });
        }
    }
}

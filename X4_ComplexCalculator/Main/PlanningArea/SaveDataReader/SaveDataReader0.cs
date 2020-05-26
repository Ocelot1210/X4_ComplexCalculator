using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.PlanningArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.PlanningArea.SaveDataReader
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
        /// 作業エリア
        /// </summary>
        protected readonly IPlanningArea _PlanningArea;


        protected readonly DoEventsExecuter _DoEventsExecuter = new DoEventsExecuter(256, 100);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="PlanningArea">作業エリア</param>
        public SaveDataReader0(IPlanningArea PlanningArea)
        {
            _PlanningArea = PlanningArea;
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

                    _PlanningArea.Title = System.IO.Path.GetFileNameWithoutExtension(Path);

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
                _DoEventsExecuter.DoEvents();
            });

            // モジュールの装備を復元
            conn.ExecQuery($"SELECT * FROM Equipments", (dr, _) =>
            {
                modules[(int)(long)dr["row"]].Module.AddEquipment(new Equipment((string)dr["EquipmentID"]));
                _DoEventsExecuter.DoEvents();
            });

            _PlanningArea.Modules.Reset(modules);
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
                var itm = _PlanningArea.Products.Where(x => x.Ware.WareID == wareID).FirstOrDefault();
                if (itm != null)
                {
                    itm.UnitPrice = (long)dr["Price"];
                }
                _DoEventsExecuter.DoEvents();
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

                var itm = _PlanningArea.Resources.Where(x => x.Ware.WareID == wareID).FirstOrDefault();
                if (itm != null)
                {
                    itm.UnitPrice = (long)dr["Price"];
                }
                _DoEventsExecuter.DoEvents();
            });
        }
    }
}

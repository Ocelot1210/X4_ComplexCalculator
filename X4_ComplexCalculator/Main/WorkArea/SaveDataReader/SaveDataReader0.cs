using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.Common.Enum;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReader
{
    /// <summary>
    /// 保存ファイル読み込みクラス(フォーマット0用)
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
                    RestoreModules(conn, progress, 90);
                    progress.Report(90);

                    // 製品価格を復元
                    RestoreProducts(conn);
                    progress.Report(95);

                    // 建造リソースを復元
                    RestoreBuildResource(conn);
                    progress.Report(100);

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
        /// <param name="conn">DB接続情報</param>
        /// <param name="maxProgress">進捗最大</param>
        protected virtual void RestoreModules(DBConnection conn, IProgress<int> progress, int maxProgress)
        {
            var records = 0;
            var moduleCnt = 0;

            // レコード数取得
            conn.ExecQuery("SELECT count(*) AS Count from Modules", (dr, _) =>
            {
                moduleCnt = (int)(long)dr[0];
            });

            records += moduleCnt;
            conn.ExecQuery("SELECT count(*) AS Count from Equipments", (dr, _) =>
            {
                records += (int)(long)dr[0];
            });


            var modules = new List<ModulesGridItem>(moduleCnt);
            var progressCnt = 1;

            // モジュールを復元
            conn.ExecQuery("SELECT ModuleID, Count FROM Modules ORDER BY Row ASC", (dr, _) =>
            {
                var module = Module.Get((string)dr["ModuleID"]);
                if (module != null)
                {
                    var mod = new ModulesGridItem(module, null, (long)dr["Count"]) { EditStatus = EditStatus.Unedited };
                    modules.Add(mod);
                }
                progress.Report((int)((double)progressCnt++ / records * maxProgress));
            });


            // モジュールの装備を復元
            conn.ExecQuery($"SELECT * FROM Equipments", (dr, _) =>
            {
                var row = (int)(long)dr["row"];
                var eqp = Equipment.Get((string)dr["EquipmentID"]);
                if (eqp != null)
                {
                    modules[row].AddEquipment(eqp);
                }
                progress.Report((int)((double)progressCnt++ / records * maxProgress));
            });


            _WorkArea.Modules.Reset(modules);
        }


        /// <summary>
        /// 製品価格を復元
        /// </summary>
        /// <param name="conn"></param>
        protected virtual void RestoreProducts(DBConnection conn)
        {
            conn.ExecQuery($"SELECT WareID, Price FROM Products", (dr, _) =>
            {
                var wareID = (string)dr["WareID"];
                var itm = _WorkArea.Products.Where(x => x.Ware.WareID == wareID).FirstOrDefault();
                if (itm != null)
                {
                    itm.UnitPrice = (long)dr["Price"];
                    itm.EditStatus = EditStatus.Unedited;
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
                    itm.EditStatus = EditStatus.Unedited;
                }
            });
        }
    }
}

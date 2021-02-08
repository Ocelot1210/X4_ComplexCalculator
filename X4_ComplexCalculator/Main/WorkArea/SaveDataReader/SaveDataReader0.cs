using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReader
{
    /// <summary>
    /// 保存ファイル読み込みクラス(フォーマット0用)
    /// </summary>
    internal class SaveDataReader0 : ISaveDataReader
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
            using var conn = new DBConnection(Path);

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
                progress.Report(98);

                // 各要素を未編集状態にする
                InitEditStatus();
                progress.Report(100);

                _WorkArea.Title = System.IO.Path.GetFileNameWithoutExtension(Path);

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                conn.Rollback();
            }
        }


        /// <summary>
        /// モジュールを復元
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        /// <param name="progress">進捗</param>
        /// <param name="maxProgress">進捗最大</param>
        protected virtual void RestoreModules(DBConnection conn, IProgress<int> progress, int maxProgress)
        {
            // レコード数取得
            var moduleCnt = conn.QuerySingle<int>("SELECT count(*) AS Count from Modules");
            var equipmentCnt = conn.QuerySingle<int>("SELECT count(*) AS Count from Equipments");
            var records = moduleCnt + equipmentCnt;


            var modules = new List<ModulesGridItem>(moduleCnt);
            var progressCnt = 1;

            // モジュールを復元
            const string sql1 = "SELECT ModuleID, Count FROM Modules ORDER BY Row ASC";
            foreach (var (moduleID, count) in conn.Query<(string, long)>(sql1))
            {
                var module = X4Database.Instance.Ware.TryGet<IX4Module>(moduleID);
                if (module is not null)
                {
                    var mod = new ModulesGridItem(module, null, count) { EditStatus = EditStatus.Unedited };
                    modules.Add(mod);
                }
                progress.Report((int)((double)progressCnt++ / records * maxProgress));
            }

            // モジュールの装備を復元
            const string sql2 = "SELECT Row, EquipmentID FROM Equipments";
            foreach (var (row, equipmentID) in conn.Query<(int, string)>(sql2))
            {
                var eqp = X4Database.Instance.Ware.TryGet<IEquipment>(equipmentID);
                if (eqp is not null)
                {
                    modules[row].AddEquipment(eqp);
                }
                progress.Report((int)((double)progressCnt++ / records * maxProgress));
            }

            _WorkArea.StationData.ModulesInfo.Modules.Reset(modules);
        }


        /// <summary>
        /// 製品価格を復元
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        protected virtual void RestoreProducts(DBConnection conn)
        {
            const string sql = "SELECT WareID, Price FROM Products";
            foreach (var (wareID, price) in conn.Query<(string, long)>(sql))
            {
                var itm = _WorkArea.StationData.ProductsInfo.Products.FirstOrDefault(x => x.Ware.ID == wareID);
                if (itm is not null)
                {
                    itm.UnitPrice = price;
                }
            }
        }


        /// <summary>
        /// 建造リソースを復元
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        protected virtual void RestoreBuildResource(DBConnection conn)
        {
            const string sql = "SELECT WareID, Price FROM BuildResources";
            foreach (var (wareID, price) in conn.Query<(string, long)>(sql))
            {
                var itm = _WorkArea.StationData.BuildResourcesInfo.BuildResources.FirstOrDefault(x => x.Ware.ID == wareID);
                if (itm is not null)
                {
                    itm.UnitPrice = price;
                }
            }
        }



        /// <summary>
        /// 編集状態を初期化
        /// </summary>
        protected virtual void InitEditStatus()
        {
            // 初期化対象
            IEnumerable<IEditable>[] initTargets =
            {
                _WorkArea.StationData.ProductsInfo.Products,
                _WorkArea.StationData.BuildResourcesInfo.BuildResources,
                _WorkArea.StationData.StorageAssignInfo.StorageAssign,
            };

            foreach (var editables in initTargets)
            {
                foreach (var editable in editables)
                {
                    editable.EditStatus = EditStatus.Unedited;
                }
            }
        }
    }
}

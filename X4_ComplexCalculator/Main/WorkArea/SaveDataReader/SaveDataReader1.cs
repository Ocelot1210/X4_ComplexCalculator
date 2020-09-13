using System;
using System.Linq;
using X4_ComplexCalculator.DB;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReader
{
    /// <summary>
    /// 保存ファイル読み込みクラス(フォーマット1用)
    /// </summary>
    internal class SaveDataReader1 : SaveDataReader0
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="WorkArea">作業エリア</param>
        public SaveDataReader1(IWorkArea WorkArea) : base(WorkArea)
        {

        }


        /// <summary>
        /// ファイル読み込み
        /// </summary>
        /// <returns>成功したか</returns>
        public override bool Load(IProgress<int> progress)
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
                progress.Report(93);

                // 建造リソースを復元
                RestoreBuildResource(conn);
                progress.Report(96);

                //保管庫割当情報を読み込み
                RestoreStorageAssignInfo(conn);
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
        /// 保管庫割当情報を読み込み
        /// </summary>
        /// <param name="conn"></param>
        protected virtual void RestoreStorageAssignInfo(DBConnection conn)
        {
            const string sql = "SELECT WareID, AllocCount FROM StorageAssign";
            foreach (var (wareID, allocCount) in conn.Query<(string, long)>(sql))
            {
                var itm = _WorkArea.StationData.StorageAssignInfo.StorageAssign.FirstOrDefault(x => x.WareID == wareID);
                if (itm != null)
                {
                    itm.AllocCount = allocCount;
                }
            }
        }
    }
}

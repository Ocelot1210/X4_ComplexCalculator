using System;
using System.Linq;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReader
{
    /// <summary>
    /// 保存ファイル読み込みクラス(フォーマット2用)
    /// </summary>
    class SaveDataReader2 : SaveDataReader1
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="WorkArea">作業エリア</param>
        public SaveDataReader2(IWorkArea WorkArea) : base(WorkArea)
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
                progress.Report(92);

                // 建造リソースを復元
                RestoreBuildResource(conn);
                progress.Report(94);

                //保管庫割当情報を読み込み
                RestoreStorageAssignInfo(conn);
                progress.Report(96);

                // ステーション設定復元
                RestoreSettings(conn, _WorkArea.StationData.Settings);
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
        /// ステーションの設定を復元
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        protected virtual void RestoreSettings(DBConnection conn, IStationSettings settings)
        {
            // 本部か
            const string sql1 = "SELECT Value FROM StationSettings WHERE Key = 'IsHeadquarters' UNION ALL SELECT 'False' LIMIT 1";
            settings.IsHeadquarters = conn.QuerySingle<string>(sql1) == bool.TrueString;

            // 日光
            const string sql2 = "SELECT Value FROM StationSettings WHERE Key = 'Sunlight' UNION ALL SELECT '100' LIMIT 1";
            var sunLightString = conn.QuerySingle<string>(sql2);
            if (int.TryParse(sunLightString, out var sunLight))
            {
                settings.Sunlight = sunLight;
            }

            // 現在の労働者数
            const string sql3 = "SELECT Value FROM StationSettings WHERE Key = 'ActualWorkforce' UNION ALL SELECT '0' LIMIT 1";
            var actualWorkforceString = conn.QuerySingle<string>(sql3);
            if (long.TryParse(actualWorkforceString, out var actualWorkforce))
            {
                settings.Workforce.Actual = actualWorkforce;
            }

            // (労働者数を)常に最大にするか
            const string sql4 = "SELECT Value FROM StationSettings WHERE key = 'AlwaysMaximumWorkforce' UNION ALL SELECT 'False' LIMIT 1";
            settings.Workforce.AlwaysMaximum = conn.QuerySingle<string>(sql4) == bool.TrueString;
        }


        /// <summary>
        /// 製品価格を復元
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        protected override void RestoreProducts(DBConnection conn)
        {
            const string sql = "SELECT WareID, Price, NoBuy, NoSell FROM Products";
            foreach (var (wareID, price, noBuy, noSell) in conn.Query<(string, long, long, long)>(sql))
            {
                var itm = _WorkArea.StationData.ProductsInfo.Products.FirstOrDefault(x => x.Ware.ID == wareID);
                if (itm is not null)
                {
                    itm.UnitPrice = price;
                    itm.NoBuy = noBuy == 1;
                    itm.NoSell = noSell == 1;
                }
            }
        }


        /// <summary>
        /// 建造リソースを復元
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        protected override void RestoreBuildResource(DBConnection conn)
        {
            const string sql = "SELECT WareID, Price, NoBuy FROM BuildResources";
            foreach (var (wareID, price, noBuy) in conn.Query<(string, long, long)>(sql))
            {
                var itm = _WorkArea.StationData.BuildResourcesInfo.BuildResources.FirstOrDefault(x => x.Ware.ID == wareID);
                if (itm is not null)
                {
                    itm.UnitPrice = price;
                    itm.NoBuy = noBuy == 1;
                }
            }
        }
    }
}

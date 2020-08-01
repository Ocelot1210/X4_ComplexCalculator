using System;
using System.Linq;
using X4_ComplexCalculator.Common.Enum;
using X4_ComplexCalculator.DB;

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
                    progress.Report(93);

                    // 建造リソースを復元
                    RestoreBuildResource(conn);
                    progress.Report(96);

                    //保管庫割当情報を読み込み
                    RestoreStorageAssignInfo(conn);

                    // ステーション設定復元
                    RestoreSettings(conn);

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
        /// ステーションの設定を復元
        /// </summary>
        /// <param name="conn"></param>
        protected virtual void RestoreSettings(DBConnection conn)
        {
            // 本部か
            conn.ExecQuery("SELECT Value FROM StationSettings WHERE Key = 'IsHeadquarters'", (dr, _) =>
            {
                var value = (string)dr["Value"];

                _WorkArea.Settings.IsHeadquarters = value == bool.TrueString;
            });

            // 日光
            conn.ExecQuery("SELECT Value FROM StationSettings WHERE Key = 'Sunlight'", (dr, _) =>
            {
                var value = (string)dr["Value"];

                if (int.TryParse(value, out var sunLight))
                {
                    _WorkArea.Settings.Sunlight = sunLight;
                }
            });

            // 現在の労働者数
            conn.ExecQuery("SELECT Value FROM StationSettings WHERE Key = 'ActualWorkforce'", (dr, _) =>
            {
                var value = (string)dr["Value"];

                if (long.TryParse(value, out var actualWorkforce))
                {
                    _WorkArea.Settings.Workforce.Actual = actualWorkforce;
                }
            });

            // (労働者数を)常に最大にするか
            conn.ExecQuery("SELECT Value FROM StationSettings WHERE key = 'AlwaysMaximumWorkforce'", (dr, _) =>
            {
                var value = (string)dr["Value"];

                _WorkArea.Settings.Workforce.AlwaysMaximum = value == bool.TrueString;
            });
        }



        /// <summary>
        /// 製品価格を復元
        /// </summary>
        /// <param name="conn"></param>
        protected override void RestoreProducts(DBConnection conn)
        {
            conn.ExecQuery($"SELECT WareID, Price, NoBuy, NoSell FROM Products", (dr, _) =>
            {
                var wareID = (string)dr["WareID"];
                var itm = _WorkArea.Products.Where(x => x.Ware.WareID == wareID).FirstOrDefault();
                if (itm != null)
                {
                    itm.UnitPrice = (long)dr["Price"];
                    itm.NoBuy     = (long)dr["NoBuy"]  == 1;
                    itm.NoSell    = (long)dr["NoSell"] == 1;

                    itm.EditStatus = EditStatus.Unedited;
                }
            });
        }


        /// <summary>
        /// 建造リソースを復元
        /// </summary>
        /// <param name="conn"></param>
        protected override void RestoreBuildResource(DBConnection conn)
        {
            conn.ExecQuery($"SELECT WareID, Price, NoBuy FROM BuildResources", (dr, _) =>
            {
                var wareID = (string)dr["WareID"];

                var itm = _WorkArea.Resources.Where(x => x.Ware.WareID == wareID).FirstOrDefault();
                if (itm != null)
                {
                    itm.UnitPrice = (long)dr["Price"];
                    itm.NoBuy = (long)dr["NoBuy"] == 1;
                    itm.EditStatus = EditStatus.Unedited;
                }
            });
        }
    }
}

using X4_ComplexCalculator.DB;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReader
{
    /// <summary>
    /// 保存ファイル読み込みクラスのインスタンス作成用クラス
    /// </summary>
    static class SaveDataReaderFactory
    {
        /// <summary>
        /// インスタンス作成
        /// </summary>
        /// <param name="path">読み込み対象ファイルパス</param>
        /// <param name="WorkArea">作業エリア</param>
        /// <returns></returns>
        public static ISaveDataReader CreateSaveDataReader(string path, IWorkArea WorkArea)
        {
            var version = GetVersion(path);
            ISaveDataReader ret;

            switch (version)
            {
                case 1:
                    ret = new SaveDataReader1(WorkArea);
                    break;

                case 2:
                    ret = new SaveDataReader2(WorkArea);
                    break;

                default:
                    ret = new SaveDataReader0(WorkArea);
                    break;
            }

            ret.Path = path;

            return ret;
        }



        /// <summary>
        /// 保存ファイルのバージョン取得
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>バージョン</returns>
        private static int GetVersion(string path)
        {
            var ret = 0;
            var tableExists = false;

            {
                using var conn = new DBConnection(path);
                conn.ExecQuery("SELECT count(*) AS Count FROM sqlite_master WHERE type = 'table' AND name = 'Common'", (dr, _) =>
                {
                    tableExists = 0L < (long)dr["Count"];
                });
            }

            if (tableExists)
            {
                using var conn = new DBConnection(path);
                conn.ExecQuery("SELECT Value FROM Common WHERE Item = 'FormatVersion'", (dr, _) =>
                {
                    ret = (int)(long)dr["Value"];
                });
            }

            return ret;
        }
    }
}

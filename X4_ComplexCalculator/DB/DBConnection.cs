using System;
using System.Data.SQLite;
using System.Windows;
using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.DB
{
    /// <summary>
    /// SQLite接続用ラッパークラス
    /// </summary>
    class DBConnection : IDisposable
    {
        #region メンバ
        /// <summary>
        /// SQLite接続用オブジェクト
        /// </summary>
        readonly SQLiteConnection conn;

        /// <summary>
        /// トランザクション用コマンド
        /// </summary>
        private SQLiteCommand transCmd;
        #endregion


        #region プロパティ(static)
        /// <summary>
        /// X4DB用
        /// </summary>
        public static DBConnection X4DB { get; private set; }

        /// <summary>
        /// プリセット/その他用DB
        /// </summary>
        public static DBConnection CommonDB { get; private set; }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dbPath">SQLite3 DBファイルパス</param>
        public DBConnection(string dbPath)
        {
            var consb = new SQLiteConnectionStringBuilder { DataSource = dbPath };
            
            conn = new SQLiteConnection(consb.ToString());
            conn.Open();
        }


        /// <summary>
        /// リソースの開放
        /// </summary>
        public void Dispose()
        {
            conn.Dispose();
        }

        /// <summary>
        /// トランザクション開始
        /// </summary>
        public void BeginTransaction()
        {
            if (transCmd == null)
            {
                transCmd = new SQLiteCommand(conn);
                transCmd.Transaction = conn.BeginTransaction();
            }
            else
            {
                throw new InvalidOperationException("前回のトランザクションが終了せずにトランザクションが開始されました。");
            }
        }


        /// <summary>
        /// コミット
        /// </summary>
        public void Commit()
        {
            transCmd.Transaction.Commit();
            transCmd.Dispose();
            transCmd = null;
        }


        /// <summary>
        /// ロールバック
        /// </summary>
        public void Rollback()
        {
            transCmd.Transaction.Rollback();
            transCmd.Dispose();
            transCmd = null;
        }


        /// <summary>
        /// クエリを実行
        /// </summary>
        /// <param name="query">クエリ</param>
        /// <param name="callback">実行結果に対する処理</param>
        /// <param name="args">可変長引数</param>
        /// <returns>結果の行数</returns>
        public int ExecQuery(string query, Action<SQLiteDataReader, object[]> callback = null, params object[] args)
        {
            int ret = 0;

            // トランザクション開始済み？
            if (transCmd != null)
            {
                ret = ExecQueryMain(transCmd, query, null, callback, args);
            }
            else
            {
                // クエリ使用準備
                using (var cmd = new SQLiteCommand(conn))
                {
                    ret = ExecQueryMain(cmd, query, null, callback, args);
                }
            }

            return ret;
        }

        /// <summary>
        /// クエリを実行
        /// </summary>
        /// <param name="query">クエリ</param>
        /// <param name="parameters">バインド変数格納用オブジェクト</param>
        /// <param name="callback">実行結果に対する処理</param>
        /// <param name="args">可変長引数</param>
        /// <returns>結果の行数</returns>
        public int ExecQuery(string query, SQLiteCommandParameters parameters, Action<SQLiteDataReader, object[]> callback, params object[] args)
        {
            int ret = 0;

            // トランザクション開始済み？
            if (transCmd != null)
            {
                ret = ExecQueryMain(transCmd, query, null, callback, args);
            }
            else
            {
                // クエリ使用準備
                using (var cmd = new SQLiteCommand(conn))
                {
                    ret = ExecQueryMain(cmd, query, parameters, callback, args);
                }
            }

            return ret;
        }



        /// <summary>
        /// クエリを実行メイン
        /// </summary>
        /// <param name="query">クエリ</param>
        /// <param name="parameters">バインド変数格納用オブジェクト</param>
        /// <param name="callback">実行結果に対する処理</param>
        /// <param name="args">可変長引数</param>
        /// <returns>結果の行数</returns>
        private int ExecQueryMain(SQLiteCommand cmd, string query, SQLiteCommandParameters parameters, Action<SQLiteDataReader, object[]> callback, params object[] args)
        {
            int ret = 0;

            // クエリを設定
            cmd.CommandText = query;

            void ExecMain()
            {
                if (callback == null)
                {
                    ret += cmd.ExecuteNonQuery();
                }
                else
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                callback.Invoke(dr, args);
                                ret++;
                            }
                        }
                    }
                }
            }

            if (parameters == null)
            {
                ExecMain();
            }
            else
            {
                foreach (var sqlParams in parameters.Parameters)
                {
                    cmd.Parameters.Clear();

                    foreach (var sqlParam in sqlParams)
                    {
                        cmd.Parameters.Add(sqlParam);
                    }

                    ExecMain();
                }
            }


            return ret;
        }


        /// <summary>
        /// 共通設定用DBオープン
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static DBConnection CreatePresetDB(string path)
        {
            DBConnection ret;

            // 共通設定用DBオープン
            ret = new DBConnection(path);

            // テーブルがなければ作る
            ret.ExecQuery("CREATE TABLE IF NOT EXISTS SelectModuleCheckStateModuleTypes(ID TEXT NOT NULL)");
            ret.ExecQuery("CREATE TABLE IF NOT EXISTS SelectModuleCheckStateModuleOwners(ID TEXT NOT NULL)");
            ret.ExecQuery("CREATE TABLE IF NOT EXISTS SelectModuleEquipmentCheckStateFactions(ID TEXT NOT NULL)");
            ret.ExecQuery("CREATE TABLE IF NOT EXISTS ModulePresets(ID TEXT NOT NULL, Name TEXT NOT NULL, EquipmentID TEXT NOT NULL)");

            return ret;
        }

        /// <summary>
        /// DBファイルを開く
        /// </summary>
        public static void Open()
        {
            var conf = Configuration.GetConfiguration();

            try
            {
                if (System.IO.File.Exists(conf["AppSettings:X4DBPath"]))
                {
                    X4DB = new DBConnection(conf["AppSettings:X4DBPath"]);
                }
                else
                {
                    MessageBox.Show(@"
DBが存在しません。
ゲームファイルよりDBを抽出する必要があります。
データ抽出画面が立ち上がるまでしばらくお待ち下さい。", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (!UpdateDB())
                    {
                        MessageBox.Show("本アプリケーションの実行にはDBの作成が必要です。\n本アプリケーションを再度実行し、DBの更新を行ってください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(-1);
                    }
                }
            }
            catch
            {
                if (MessageBox.Show(@"
DBのオープンに失敗しました。
ゲームファイルよりDBを抽出する事で起動可能になるかもしれません。
DBを更新しますか？", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    if (!UpdateDB())
                    {
                        MessageBox.Show("本アプリケーションの実行にはDBの更新が必要です。\n本アプリケーションを再度実行し、DBの更新を行ってください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(-1);
                    }
                }
                else
                {
                    MessageBox.Show("本アプリケーションの実行にはDBの更新が必要です。\n本アプリケーションを再度実行し、DBの更新を行ってください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(-1);
                }
            }

            
            CommonDB = CreatePresetDB(conf["AppSettings:CommonDBPath"]);
        }


        /// <summary>
        /// DBを更新
        /// </summary>
        /// <returns></returns>
        public static bool UpdateDB()
        {
            X4DB?.Dispose();

            var conf = Configuration.GetConfiguration();

            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, conf["AppSettings:X4DBPath"]);

            var proc = System.Diagnostics.Process.Start(conf["AppSettings:ExporterExePath"], $"-o \"{System.IO.Path.GetFullPath(path)}\"");
            proc.WaitForExit();

            if (System.IO.File.Exists(conf["AppSettings:X4DBPath"]))
            {
                X4DB = new DBConnection(conf["AppSettings:X4DBPath"]);
                return true;
            }

            return false;
        }
    }
}

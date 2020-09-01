using System;
using System.Data.SQLite;

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
        private SQLiteCommand? _TransCommand;
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
            _TransCommand?.Dispose();
        }

        /// <summary>
        /// トランザクション開始
        /// </summary>
        public void BeginTransaction()
        {
            if (_TransCommand == null)
            {
                _TransCommand = new SQLiteCommand(conn);
                _TransCommand.Transaction = conn.BeginTransaction();
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
            if (_TransCommand == null)
            {
                throw new InvalidOperationException();
            }
            _TransCommand.Transaction.Commit();
            _TransCommand.Dispose();
            _TransCommand = null;
        }


        /// <summary>
        /// ロールバック
        /// </summary>
        public void Rollback()
        {
            if (_TransCommand == null)
            {
                throw new InvalidOperationException();
            }
            _TransCommand.Transaction.Rollback();
            _TransCommand.Dispose();
            _TransCommand = null;
        }


        /// <summary>
        /// クエリを実行
        /// </summary>
        /// <param name="query">クエリ</param>
        /// <param name="callback">実行結果に対する処理</param>
        /// <param name="args">可変長引数</param>
        /// <returns>結果の行数</returns>
        public int ExecQuery(string query, Action<SQLiteDataReader, object[]>? callback = null, params object[] args)
        {
            int ret = 0;

            // トランザクション開始済み？
            if (_TransCommand != null)
            {
                ret = ExecQueryMain(_TransCommand, query, null, callback, args);
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
        public int ExecQuery(string query, SQLiteCommandParameters parameters, Action<SQLiteDataReader, object[]>? callback = null, params object[] args)
        {
            int ret = 0;

            // トランザクション開始済み？
            if (_TransCommand != null)
            {
                ret = ExecQueryMain(_TransCommand, query, parameters, callback, args);
            }
            else
            {
                // クエリ使用準備
                using var cmd = new SQLiteCommand(conn);
                ret = ExecQueryMain(cmd, query, parameters, callback, args);
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
        private int ExecQueryMain(SQLiteCommand cmd, string query, SQLiteCommandParameters? parameters, Action<SQLiteDataReader, object[]>? callback, params object[] args)
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
                    using var dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            callback(dr, args);
                            ret++;
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
    }
}

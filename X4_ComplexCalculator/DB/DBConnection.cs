using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

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
        private readonly SQLiteConnection _Connection;


        /// <summary>
        /// トランザクション用コマンド
        /// </summary>
        private SQLiteTransaction? _Transaction;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dbPath">SQLite3 DBファイルパス</param>
        public DBConnection(string dbPath)
        {
            var consb = new SQLiteConnectionStringBuilder { DataSource = dbPath };

            _Connection = new SQLiteConnection(consb.ToString());
            _Connection.Open();
        }


        /// <summary>
        /// リソースの開放
        /// </summary>
        public void Dispose()
        {
            _Connection.Dispose();
            _Transaction?.Dispose();
        }


        /// <summary>
        /// トランザクション開始
        /// </summary>
        public void BeginTransaction()
        {
            if (_Transaction != null)
            {
                throw new InvalidOperationException("前回のトランザクションが終了せずにトランザクションが開始されました。");
            }
            _Transaction = _Connection.BeginTransaction();
        }


        /// <summary>
        /// コミット
        /// </summary>
        public void Commit()
        {
            if (_Transaction == null)
            {
                throw new InvalidOperationException();
            }
            _Transaction.Commit();
            _Transaction.Dispose();
            _Transaction = null;
        }


        /// <summary>
        /// ロールバック
        /// </summary>
        public void Rollback()
        {
            if (_Transaction == null)
            {
                throw new InvalidOperationException();
            }
            _Transaction.Rollback();
            _Transaction.Dispose();
            _Transaction = null;
        }


        /// <summary>
        /// クエリを実行
        /// </summary>
        /// <param name="query">クエリ</param>
        /// <param name="callback">実行結果に対する処理</param>
        /// <param name="args">可変長引数</param>
        /// <returns>結果の行数</returns>
        public int ExecQuery(string query,
                             Action<SQLiteDataReader, object[]>? callback = null,
                             params object[] args)
            => ExecQueryMain(query, null, callback, args);


        /// <summary>
        /// クエリを実行
        /// </summary>
        /// <param name="query">クエリ</param>
        /// <param name="parameters">バインド変数格納用オブジェクト</param>
        /// <param name="callback">実行結果に対する処理</param>
        /// <param name="args">可変長引数</param>
        /// <returns>結果の行数</returns>
        public int ExecQuery(string query,
                             SQLiteCommandParameters parameters,
                             Action<SQLiteDataReader, object[]>? callback = null,
                             params object[] args)
            => parameters.Parameters
                .Select(sqlParams => ExecQueryMain(query, sqlParams, callback, args))
                .Sum();


        /// <summary>
        /// クエリを実行メイン
        /// </summary>
        /// <param name="query">クエリ</param>
        /// <param name="parameters">バインド変数格納用オブジェクト</param>
        /// <param name="callback">実行結果に対する処理</param>
        /// <param name="args">可変長引数</param>
        /// <returns>結果の行数</returns>
        private int ExecQueryMain(string query,
                                  IEnumerable<SQLiteParameter>? parameters,
                                  Action<SQLiteDataReader, object[]>? callback,
                                  params object[] args)
        {
            using var cmd = new SQLiteCommand(query, _Connection, _Transaction);

            foreach (var sqlParam in parameters ?? Enumerable.Empty<SQLiteParameter>())
            {
                cmd.Parameters.Add(sqlParam);
            }

            if (callback == null) return cmd.ExecuteNonQuery();

            using var dr = cmd.ExecuteReader();
            int ret = 0;
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    callback(dr, args);
                    ret++;
                }
            }
            return ret;
        }
    }
}

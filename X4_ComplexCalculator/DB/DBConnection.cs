using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace X4_ComplexCalculator.DB;

/// <summary>
/// SQLite接続用ラッパークラス
/// </summary>
class DBConnection : IDisposable
{
    #region メンバ
    /// <summary>
    /// DB接続オブジェクト
    /// </summary>
    protected readonly IDbConnection _connection;


    /// <summary>
    /// トランザクション用コマンド
    /// </summary>
    private IDbTransaction? _transaction;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="dbPath">SQLite3 DBファイルパス</param>
    public DBConnection(string dbPath)
    {
        var consb = new SQLiteConnectionStringBuilder { DataSource = dbPath };

        _connection = new SQLiteConnection(consb.ToString());
        _connection.Open();
    }


    /// <summary>
    /// リソースの開放
    /// </summary>
    public void Dispose()
    {
        _connection.Dispose();
        _transaction?.Dispose();
    }


    /// <summary>
    /// 指定の関数を同一トランザクションとして処理する
    /// </summary>
    public void BeginTransaction(Action<DBConnection> action)
    {
        BeginTransaction();
        try
        {
            action(this);
            Commit();
        }
        catch
        {
            Rollback();
            throw;
        }
    }


    /// <summary>
    /// トランザクション開始
    /// </summary>
    public void BeginTransaction()
    {
        if (_transaction is not null)
        {
            throw new InvalidOperationException("前回のトランザクションが終了せずにトランザクションが開始されました。");
        }
        
        _transaction = _connection.BeginTransaction();
    }


    /// <summary>
    /// コミット
    /// </summary>
    public void Commit()
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException();
        }
        _transaction.Commit();
        _transaction.Dispose();
        _transaction = null;
    }


    /// <summary>
    /// ロールバック
    /// </summary>
    public void Rollback()
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException();
        }
        _transaction.Rollback();
        _transaction.Dispose();
        _transaction = null;
    }


    /// <summary>
    /// クエリを実行する
    /// </summary>
    /// <param name="sql">実行するクエリ</param>
    /// <param name="param">クエリに埋め込むパラメータ</param>
    /// <returns>マッピング済みのクエリ実行結果</returns>
    public int Execute(string sql, object? param = null)
        => _connection.Execute(sql, param, _transaction);


    /// <summary>
    /// クエリを実行し、結果を指定の型にマッピングする
    /// </summary>
    /// <typeparam name="T">クエリ実行結果のマッピング先</typeparam>
    /// <param name="sql">実行するクエリ</param>
    /// <param name="param">クエリに埋め込むパラメータ</param>
    /// <returns>マッピング済みのクエリ実行結果</returns>
    public IEnumerable<T> Query<T>(string sql, object? param = null)
        => _connection.Query<T>(sql, param);


    /// <summary>
    /// クエリを実行し、結果が 1 行の場合のみ指定の型にマッピングする
    /// </summary>
    /// <typeparam name="T">クエリ実行結果のマッピング先</typeparam>
    /// <param name="sql">実行するクエリ</param>
    /// <param name="param">クエリに埋め込むパラメータ</param>
    /// <returns>マッピング済みのクエリ実行結果</returns>
    public T QuerySingle<T>(string sql, object? param = null)
        => _connection.QuerySingle<T>(sql, param, _transaction);
}

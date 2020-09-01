﻿using System;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB.X4DB;
using X4_DataExporterWPF.DataExportWindow;

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


        #region スタティックメンバ
        /// <summary>
        /// X4DB用
        /// </summary>
        private static DBConnection? _X4DB;
        #endregion


        #region プロパティ(static)
        /// <summary>
        /// X4DB用
        /// </summary>
        public static DBConnection X4DB
        {
            get => _X4DB ?? throw new InvalidOperationException();
            private set => _X4DB = value;
        }
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


        /// <summary>
        /// DBファイルを開く
        /// </summary>
        public static void Open()
        {
            var conf = Configuration.GetConfiguration();
            var x4DBPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? "", conf["AppSettings:X4DBPath"]));

            try
            {
                // DB格納先フォルダが無ければ作る
                Directory.CreateDirectory(Path.GetDirectoryName(x4DBPath));

                if (File.Exists(x4DBPath))
                {
                    // X4DBが存在する場合

                    X4DB = new DBConnection(x4DBPath);
                    if (X4_DataExporterWPF.Export.CommonExporter.CURRENT_FORMAT_VERSION == GetDBVersion())
                    {
                        // 想定するDBのフォーマットと実際のフォーマットが同じ場合
                        InitX4DB();
                    }
                    else
                    {
                        // 想定するDBのフォーマットと実際のフォーマットが異なる場合
                        // DB更新を要求

                        LocalizedMessageBox.Show("Lang:OldFormatMessage", "Lang:Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        if (!UpdateDB() || GetDBVersion() != X4_DataExporterWPF.Export.CommonExporter.CURRENT_FORMAT_VERSION)
                        {
                            // DB更新を要求してもフォーマットが変わらない場合

                            LocalizedMessageBox.Show("Lang:DBUpdateRequestMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            Environment.Exit(-1);
                        }
                    }
                }
                else
                {
                    // X4DBが存在しない場合

                    // X4DBの作成を要求する
                    LocalizedMessageBox.Show("Lang:DBExtractionRequestMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (!UpdateDB())
                    {
                        LocalizedMessageBox.Show("Lang:DBMakeRequestMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(-1);
                    }
                }
            }
            catch
            {
                // X4DBを開く際にエラーがあった場合、DB更新を提案する
                if (LocalizedMessageBox.Show("Lang:DBOpenFailMessage", "Lang:Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    // 提案が受け入れられた場合、DB更新
                    if (!UpdateDB())
                    {
                        // DB更新失敗
                        LocalizedMessageBox.Show("Lang:DBUpdateRequestMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(-1);
                    }
                }
                else
                {
                    // 提案が受け入れられなかった場合
                    LocalizedMessageBox.Show("Lang:DBUpdateRequestMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(-1);
                }
            }
        }


        /// <summary>
        /// DBのバージョン取得
        /// </summary>
        private static long GetDBVersion()
        {
            var ret = 0L;
            var tableExists = false;

            {
                X4DB.ExecQuery("SELECT count(*) AS Count FROM sqlite_master WHERE type = 'table' AND name = 'Common'", (dr, _) =>
                {
                    tableExists = 0L < (long)dr["Count"];
                });
            }

            if (tableExists)
            {
                X4DB.ExecQuery("SELECT Value FROM Common WHERE Item = 'FormatVersion'", (dr, _) =>
                {
                    ret = (long)dr["Value"];
                });
            }

            return ret;
        }



        /// <summary>
        /// DBを更新
        /// </summary>
        /// <returns></returns>
        public static bool UpdateDB()
        {
            _X4DB?.Dispose();

            var conf = Configuration.GetConfiguration();

            var dbFilePath = conf["AppSettings:X4DBPath"];
            if (!Path.IsPathRooted(dbFilePath))
            {
                // DBファイルが相対パスの場合、実行ファイルのパスをベースにパスを正規化する
                var exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
                dbFilePath = Path.GetFullPath(Path.Combine(exeDir, conf["AppSettings:X4DBPath"]));
            }

            DataExportWindow.ShowDialog(GetX4InstallDirectory(), dbFilePath);

            if (File.Exists(dbFilePath))
            {
                X4DB = new DBConnection(dbFilePath);
                InitX4DB();
                return true;
            }

            return false;
        }


        /// <summary>
        /// X4のインストール先フォルダを取得する
        /// </summary>
        /// <returns>X4のインストール先フォルダパス</returns>
        private static string GetX4InstallDirectory()
        {
            // アプリケーションのアンインストール情報が保存されている場所
            var location = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

            // レジストリ情報の取得を試みる
            RegistryKey? parent = Registry.LocalMachine.OpenSubKey(location, false);
            if (parent == null)
            {
                // だめだった場合諦める
                return "";
            }

            var ret = "";

            // 子のレジストリの名前の数だけ処理をする
            // Steam以外(GOG等)からインストールされる事を考慮してレジストリのキーを決め打ちにしないで全部探す
            foreach (var subKeyName in parent.GetSubKeyNames())
            {
                // 子のレジストリの情報を取得する
                RegistryKey? child = Registry.LocalMachine.OpenSubKey(@$"{location}\{subKeyName}", false);
                if (child == null)
                {
                    // 取得に失敗したら次のレジストリを見に行く
                    continue;
                }

                // 表示名を保持しているオブジェクトを取得する
                object value = child.GetValue("DisplayName");
                if (value == null)
                {
                    // 取得に失敗したら次のレジストリを見に行く
                    continue;
                }

                if (value.ToString() == "X4: Foundations")
                {
                    ret = child.GetValue("InstallLocation")?.ToString() ?? "";
                    break;
                }
            }

            return ret;
        }


        /// <summary>
        /// X4DBを初期化
        /// </summary>
        private static void InitX4DB()
        {
            DB.X4DB.Size.Init();
            ModuleType.Init();
            Race.Init();
            Faction.Init();
            EquipmentType.Init();
            Equipment.Init();
            TransportType.Init();
            WareGroup.Init();
            Ware.Init();
            ModuleType.Init();
            ModuleProduction.Init();
            ModuleEquipment.Init();
            Module.Init();
        }
    }
}

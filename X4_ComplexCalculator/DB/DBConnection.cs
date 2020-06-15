using System;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB.X4DB;

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


        /// <summary>
        /// X4DB用
        /// </summary>
        private static DBConnection? _X4DB;


        /// <summary>
        /// プリセット/その他用DB
        /// </summary>
        private static DBConnection? _CommonDB;
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

        /// <summary>
        /// プリセット/その他用DB
        /// </summary>
        public static DBConnection CommonDB
        {
            get => _CommonDB ?? throw new InvalidOperationException();
            private set => _CommonDB = value;
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
                ret = ExecQueryMain(_TransCommand, query, null, callback, args);
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
            ret.ExecQuery("CREATE TABLE IF NOT EXISTS ModulePresets(ModuleID TEXT NOT NULL, PresetID INTEGER NOT NULL, PresetName TEXT NOT NULL)");
            ret.ExecQuery("CREATE TABLE IF NOT EXISTS ModulePresetsEquipment(ModuleID TEXT NOT NULL, PresetID INTEGER NOT NULL, EquipmentID TEXT NOT NULL, EquipmentType TEXT NOT NULL)");
            ret.ExecQuery("CREATE TABLE IF NOT EXISTS WorkAreaLayouts(LayoutID INTEGER NOT NULL, LayoutName TEXT NOT NULL, IsChecked INTEGER DEFAULT 0, Layout BLOB NOT NULL)");
            ret.ExecQuery("CREATE TABLE IF NOT EXISTS OpenedFiles(Path TEXT NOT NULL)");

            return ret;
        }

        /// <summary>
        /// DBファイルを開く
        /// </summary>
        public static void Open()
        {
            var conf = Configuration.GetConfiguration();
            var x4DBPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? "", conf["AppSettings:X4DBPath"]);

            try
            {
                if (File.Exists(x4DBPath))
                {
                    X4DB = new DBConnection(x4DBPath);
                    if (0 < GetDBVersion())
                    {
                        InitX4DB();
                    }
                    else
                    {
                        LocalizedMessageBox.Show("Lang:OldFormatMessage", "Lang:Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                        if (!UpdateDB() || GetDBVersion() < 1)
                        {
                            LocalizedMessageBox.Show("Lang:DBUpdateRequestMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            Environment.Exit(-1);
                        }
                    }
                }
                else
                {
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
                if (LocalizedMessageBox.Show("Lang:DBOpenFailMessage", "Lang:Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    if (!UpdateDB())
                    {
                        LocalizedMessageBox.Show("Lang:DBUpdateRequestMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(-1);
                    }
                }
                else
                {
                    LocalizedMessageBox.Show("Lang:DBUpdateRequestMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(-1);
                }
            }
            
            CommonDB = CreatePresetDB(Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? "", conf["AppSettings:CommonDBPath"]));
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
            X4DB?.Dispose();

            var conf = Configuration.GetConfiguration();
            
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? "", conf["AppSettings:X4DBPath"]);

            var proc = System.Diagnostics.Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? "", conf["AppSettings:ExporterExePath"]), $"-o \"{path}\"");
            proc.WaitForExit();

            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? "", conf["AppSettings:X4DBPath"]);
            if (File.Exists(path))
            {
                X4DB = new DBConnection(path);
                InitX4DB();
                return true;
            }

            return false;
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

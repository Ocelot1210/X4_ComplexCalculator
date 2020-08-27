using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using Dapper;
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
        /// 実行中のトランザクション
        /// </summary>
        private SQLiteTransaction? _Transaction;
        #endregion


        #region スタティックメンバ
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
            _Transaction = conn.BeginTransaction();
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
        public int ExecQuery(string query, Action<IDataReader, object[]>? callback = null, params object[] args)
            => ExecQuery(query, null, callback, args);


        /// <summary>
        /// クエリを実行
        /// </summary>
        /// <param name="query">クエリ</param>
        /// <param name="parameters">バインド変数格納用オブジェクト</param>
        /// <param name="callback">実行結果に対する処理</param>
        /// <param name="args">可変長引数</param>
        /// <returns>結果の行数</returns>
        public int ExecQuery(string query, SQLiteCommandParameters? parameters, Action<IDataReader, object[]>? callback = null, params object[] args)
        {
            var param = parameters?.AsDynamicParameters();

            if (callback == null) return conn.Execute(query, param, _Transaction);

            using var dr = conn.ExecuteReader(query, param, _Transaction);
            int ret = 0;
            while (dr.Read())
            {
                callback(dr, args);
                ret++;
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

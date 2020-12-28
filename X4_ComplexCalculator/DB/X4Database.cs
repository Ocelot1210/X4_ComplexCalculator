using System;
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
    /// X4 データベースの読み込みを行うクラス
    /// </summary>
    class X4Database : DBConnection
    {
        #region スタティックメンバ
        /// <summary>
        /// インスタンス
        /// </summary>
        public static X4Database? _Instance;
        #endregion


        #region プロパティ(static)
        /// <summary>
        /// インスタンス
        /// </summary>
        public static X4Database Instance
            => _Instance ?? throw new InvalidOperationException();
        #endregion


        /// <summary>
        /// X4 データベースを開く
        /// </summary>
        /// <param name="dbPath">データベースの絶対パス</param>
        private X4Database(string dbPath) : base(dbPath) { }


        /// <summary>
        /// X4 データベースを開く
        /// </summary>
        /// <returns>X4 データベース</returns>
        public static void Open()
        {
            if (_Instance is not null) return;

            var config = Configuration.GetConfiguration();
            var basePath = AppDomain.CurrentDomain.BaseDirectory ?? "";
            var dbPath = Path.Combine(basePath, config["AppSettings:X4DBPath"]);

            try
            {
                // DB格納先フォルダが無ければ作る
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

                if (File.Exists(dbPath))
                {
                    // X4DBが存在する場合

                    _Instance = new X4Database(dbPath);
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
                Instance.ExecQuery("SELECT count(*) AS Count FROM sqlite_master WHERE type = 'table' AND name = 'Common'", (dr, _) =>
                {
                    tableExists = 0L < (long)dr["Count"];
                });
            }

            if (tableExists)
            {
                Instance.ExecQuery("SELECT Value FROM Common WHERE Item = 'FormatVersion'", (dr, _) =>
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
            _Instance?.Dispose();

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
                _Instance = new X4Database(dbFilePath);
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
            const string location = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

            // レジストリ情報の取得を試みる
            RegistryKey? parent = Registry.LocalMachine.OpenSubKey(location, false);
            if (parent is null)
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
                if (child is null)
                {
                    // 取得に失敗したら次のレジストリを見に行く
                    continue;
                }

                // 表示名を保持しているオブジェクトを取得する
                var value = child.GetValue("DisplayName");
                if (value is null)
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
            X4Size.Init();
            ModuleType.Init();
            Race.Init();
            Faction.Init();
            EquipmentType.Init();
            Equipment.Init();
            TransportType.Init();
            WareGroup.Init();
            WareEffect.Init();
            Ware.Init();
            WareProduction.Init();
            ModuleType.Init();
            ModuleProduction.Init();
            ModuleProduct.Init();
            Module.Init();
        }
    }
}

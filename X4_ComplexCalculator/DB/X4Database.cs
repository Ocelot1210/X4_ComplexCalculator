using System;
using System.IO;
using System.Windows;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB.X4DB.Manager;
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


        #region メンバ
        /// <summary>
        /// サイズ情報一覧
        /// </summary>
        private X4SizeManager? _X4Size;


        /// <summary>
        /// 種族情報一覧
        /// </summary>
        private RaceManager? _Race;


        /// <summary>
        /// 派閥情報一覧
        /// </summary>
        private FactionManager? _Faction;


        /// <summary>
        /// カーゴ種別一覧
        /// </summary>
        private TransportTypeManager? _TransportType;


        /// <summary>
        /// 装備種別一覧
        /// </summary>
        private EquipmentTypeManager? _EquipmentType;


        /// <summary>
        /// ウェア一覧
        /// </summary>
        private WareManager? _Ware;
        #endregion


        #region スタティックプロパティ
        /// <summary>
        /// インスタンス
        /// </summary>
        public static X4Database Instance
            => _Instance ?? throw new InvalidOperationException();
        #endregion



        #region プロパティ
        /// <summary>
        /// サイズ情報一覧
        /// </summary>
        public X4SizeManager X4Size => _X4Size ?? throw new InvalidOperationException();


        /// <summary>
        /// 種族情報一覧
        /// </summary>
        public RaceManager Race => _Race ?? throw new InvalidOperationException();


        /// <summary>
        /// 派閥情報一覧
        /// </summary>
        public FactionManager Faction => _Faction ?? throw new InvalidOperationException();


        /// <summary>
        /// カーゴ種別一覧
        /// </summary>
        public TransportTypeManager TransportType => _TransportType ?? throw new InvalidOperationException();


        /// <summary>
        /// 装備種別一覧
        /// </summary>
        public EquipmentTypeManager EquipmentType => _EquipmentType ?? throw new InvalidOperationException();


        /// <summary>
        /// ウェア一覧
        /// </summary>
        public WareManager Ware => _Ware ?? throw new InvalidOperationException();
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

            var basePath = AppDomain.CurrentDomain.BaseDirectory ?? "";
            var dbPath = Path.Combine(basePath, Configuration.Instance.X4DBPath);

            try
            {
                // DB格納先フォルダが無ければ作る
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

                if (File.Exists(dbPath))
                {
                    // X4DBが存在する場合

                    _Instance = new X4Database(dbPath);
                    if (X4_DataExporterWPF.Export.CommonExporter.CURRENT_FORMAT_VERSION == _Instance.GetDBVersion())
                    {
                        // 想定するDBのフォーマットと実際のフォーマットが同じ場合
                        _Instance.Init();
                    }
                    else
                    {
                        // 想定するDBのフォーマットと実際のフォーマットが異なる場合
                        // DB更新を要求

                        LocalizedMessageBox.Show("Lang:DB_OldFormatMessage", "Lang:Common_MessageBoxTitle_Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        if (!UpdateDB() || _Instance.GetDBVersion() != X4_DataExporterWPF.Export.CommonExporter.CURRENT_FORMAT_VERSION)
                        {
                            // DB更新を要求してもフォーマットが変わらない場合

                            LocalizedMessageBox.Show("Lang:DB_UpdateRequestMessage", "Lang:Common_MessageBoxTitle_Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            Environment.Exit(-1);
                        }
                    }
                }
                else
                {
                    // X4DBが存在しない場合

                    // X4DBの作成を要求する
                    LocalizedMessageBox.Show("Lang:DB_ExtractionRequestMessage", "Lang:Common_MessageBoxTitle_Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (!UpdateDB())
                    {
                        LocalizedMessageBox.Show("Lang:DB_MakeRequestMessage", "Lang:Common_MessageBoxTitle_Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(-1);
                    }
                }
            }
            catch
            {
                // X4DBを開く際にエラーがあった場合、DB更新を提案する
                if (LocalizedMessageBox.Show("Lang:DB_OpenFailMessage", "Lang:Common_MessageBoxTitle_Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    // 提案が受け入れられた場合、DB更新
                    if (!UpdateDB())
                    {
                        // DB更新失敗
                        LocalizedMessageBox.Show("Lang:DB_UpdateRequestMessage", "Lang:Common_MessageBoxTitle_Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(-1);
                    }
                }
                else
                {
                    // 提案が受け入れられなかった場合
                    LocalizedMessageBox.Show("Lang:DB_UpdateRequestMessage", "Lang:Common_MessageBoxTitle_Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(-1);
                }
            }
        }


        /// <summary>
        /// DBのバージョン取得
        /// </summary>
        private long GetDBVersion()
        {
            const string sql1 = "SELECT count(*) AS Count FROM sqlite_master WHERE type = 'table' AND name = 'Common'";
            var tableExists = 0 < QuerySingle<long>(sql1);

            if (!tableExists)
            {
                return 0;
            }

            const string sql2 = "SELECT Value FROM Common WHERE Item = 'FormatVersion' UNION ALL SELECT 0 LIMIT 1";
            return QuerySingle<long>(sql2);
        }


        /// <summary>
        /// DBを更新
        /// </summary>
        /// <returns></returns>
        public static bool UpdateDB()
        {
            _Instance?.Dispose();

            var dbFilePath = Configuration.Instance.X4DBPath;
            if (!Path.IsPathRooted(dbFilePath))
            {
                // DBファイルが相対パスの場合、実行ファイルのパスをベースにパスを正規化する
                var exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
                dbFilePath = Path.GetFullPath(Path.Combine(exeDir, Configuration.Instance.X4DBPath));
            }
            
            DataExportWindow.ShowDialog(LibX4.X4Path.GetX4InstallDirectory(), dbFilePath);

            if (File.Exists(dbFilePath))
            {
                _Instance = new X4Database(dbFilePath);
                _Instance.Init();
                return true;
            }

            return false;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        private void Init()
        {
            _X4Size  = new(_Connection);
            _Race    = new(_Connection);
            _Faction = new(_Connection, _Race);
            _TransportType = new(_Connection);
            _EquipmentType = new(_Connection);
            _Ware = new(_Connection, _TransportType);
        }
    }
}

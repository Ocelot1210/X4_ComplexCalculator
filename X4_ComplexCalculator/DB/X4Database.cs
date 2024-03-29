﻿using System;
using System.IO;
using System.Windows;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Dialog.MessageBoxes;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB.X4DB.Manager;
using X4_DataExporterWPF.DataExportWindow;

namespace X4_ComplexCalculator.DB;

/// <summary>
/// X4 データベースの読み込みを行うクラス
/// </summary>
class X4Database : DBConnection
{
    #region スタティックメンバ
    /// <summary>
    /// インスタンス
    /// </summary>
    private static X4Database? _Instance;
    #endregion


    #region メンバ
    /// <summary>
    /// サイズ情報一覧
    /// </summary>
    private X4SizeManager? _x4Size;


    /// <summary>
    /// 種族情報一覧
    /// </summary>
    private RaceManager? _race;


    /// <summary>
    /// 派閥情報一覧
    /// </summary>
    private FactionManager? _faction;


    /// <summary>
    /// カーゴ種別一覧
    /// </summary>
    private TransportTypeManager? _transportType;


    /// <summary>
    /// 装備種別一覧
    /// </summary>
    private EquipmentTypeManager? _equipmentType;


    /// <summary>
    /// ウェア一覧
    /// </summary>
    private WareManager? _ware;
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
    public X4SizeManager X4Size => _x4Size ?? throw new InvalidOperationException();


    /// <summary>
    /// 種族情報一覧
    /// </summary>
    public RaceManager Race => _race ?? throw new InvalidOperationException();


    /// <summary>
    /// 派閥情報一覧
    /// </summary>
    public FactionManager Faction => _faction ?? throw new InvalidOperationException();


    /// <summary>
    /// カーゴ種別一覧
    /// </summary>
    public TransportTypeManager TransportType => _transportType ?? throw new InvalidOperationException();


    /// <summary>
    /// 装備種別一覧
    /// </summary>
    public EquipmentTypeManager EquipmentType => _equipmentType ?? throw new InvalidOperationException();


    /// <summary>
    /// ウェア一覧
    /// </summary>
    public WareManager Ware => _ware ?? throw new InvalidOperationException();
    #endregion



    /// <summary>
    /// X4 データベースを開く
    /// </summary>
    /// <param name="dbPath">データベースの絶対パス</param>
    private X4Database(string dbPath) : base(dbPath) { }


    /// <summary>
    /// X4 データベースを開く
    /// </summary>
    /// <param name="localizedMessageBox">メッセージボックス表示用</param>
    /// <returns>X4 データベース</returns>
    public static void Open(ILocalizedMessageBox localizedMessageBox)
    {
        if (_Instance is not null) return;

        var basePath = AppDomain.CurrentDomain.BaseDirectory ?? "";
        var dbPath = Path.Join(basePath, Configuration.Instance.X4DBPath);

        try
        {
            // DB格納先フォルダが無ければ作る
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

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

                    localizedMessageBox.Warn("Lang:DB_OldFormatMessage", "Lang:Common_MessageBoxTitle_Warning");
                    if (UpdateDB() != UpdateDbStatus.Succeeded)
                    {
                        // DB更新を要求してもフォーマットが変わらない場合

                        localizedMessageBox.Error("Lang:DB_UpdateRequestMessage", "Lang:Common_MessageBoxTitle_Error");
                        Environment.Exit(-1);
                    }
                }
            }
            else
            {
                // X4DBが存在しない場合

                // X4DBの作成を要求する
                localizedMessageBox.Ok("Lang:DB_ExtractionRequestMessage", "Lang:Common_MessageBoxTitle_Confirmation");
                if (UpdateDB() != UpdateDbStatus.Succeeded)
                {
                    localizedMessageBox.Error("Lang:DB_MakeRequestMessage", "Lang:Common_MessageBoxTitle_Error");
                    Environment.Exit(-1);
                }
            }
        }
        catch
        {
            // X4DBを開く際にエラーがあった場合、DB更新を提案する
            if (localizedMessageBox.YesNoWarn("Lang:DB_OpenFailMessage", "Lang:Common_MessageBoxTitle_Error", LocalizedMessageBoxResult.Yes) == LocalizedMessageBoxResult.Yes)
            {
                // 提案が受け入れられた場合、DB更新
                if (UpdateDB() != UpdateDbStatus.Succeeded)
                {
                    // DB更新失敗
                    localizedMessageBox.Error("Lang:DB_UpdateRequestMessage", "Lang:Common_MessageBoxTitle_Error");
                    Environment.Exit(-1);
                }
            }
            else
            {
                // 提案が受け入れられなかった場合
                localizedMessageBox.Error("Lang:DB_UpdateRequestMessage", "Lang:Common_MessageBoxTitle_Error");
                Environment.Exit(-1);
            }
        }
    }


    /// <summary>
    /// DBのバージョン取得
    /// </summary>
    private long GetDBVersion()
    {
        const string SQL_1 = "SELECT count(*) AS Count FROM sqlite_master WHERE type = 'table' AND name = 'Common'";
        var tableExists = 0 < QuerySingle<long>(SQL_1);

        if (!tableExists)
        {
            return 0;
        }

        const string SQL_2 = "SELECT Value FROM Common WHERE Item = 'FormatVersion' UNION ALL SELECT 0 LIMIT 1";
        return QuerySingle<long>(SQL_2);
    }


    /// <summary>
    /// DB 更新結果
    /// </summary>
    internal enum UpdateDbStatus
    {
        NoChange = 0,
        Succeeded = 1,
        Failed = 2,
    }


    /// <summary>
    /// DBを更新
    /// </summary>
    /// <returns></returns>
    public static UpdateDbStatus UpdateDB()
    {
        _Instance?.Dispose();

        var dbFilePath = Configuration.Instance.X4DBPath;
        if (!Path.IsPathRooted(dbFilePath))
        {
            // DBファイルが相対パスの場合、実行ファイルのパスをベースにパスを正規化する
            var exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
            dbFilePath = Path.GetFullPath(Path.Combine(exeDir, Configuration.Instance.X4DBPath));
        }

        var prevTimestamp = File.Exists(dbFilePath) ? File.GetLastWriteTime(dbFilePath) : DateTime.MinValue;

        DataExportWindow.ShowDialog(LibX4.X4Path.GetX4InstallDirectory(), dbFilePath);

        if (File.Exists(dbFilePath))
        {
            _Instance = new X4Database(dbFilePath);
            _Instance.Init();

            return (File.GetLastWriteTime(dbFilePath) == prevTimestamp) ? UpdateDbStatus.NoChange : UpdateDbStatus.Succeeded; ;
        }

        return UpdateDbStatus.Failed;
    }


    /// <summary>
    /// 初期化
    /// </summary>
    /// </summary>
    private void Init()
    {
        _x4Size = new(_connection);
        _race = new(_connection);
        _faction = new(_connection, _race);
        _transportType = new(_connection);
        _equipmentType = new(_connection);
        _ware = new(_connection, _transportType);
    }
}

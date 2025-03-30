using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Linq;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReaders;

/// <summary>
/// 保存ファイル読み込みクラス(フォーマット2用)
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="messenger">メッセージ通知用</param>
/// <param name="workArea">作業エリア</param>
class SaveDataReader2(IMessenger messenger, IWorkArea workArea) : SaveDataReader1(messenger, workArea)
{
    /// <summary>
    /// ファイル読み込み
    /// </summary>
    /// <returns>成功したか</returns>
    public override bool Load(IProgress<int> progress)
    {
        using var conn = new DBConnection(Path);

        try
        {
            conn.BeginTransaction();

            // モジュール復元
            RestoreModules(conn, progress, 90);
            progress.Report(90);

            // 製品価格を復元
            RestoreProducts(conn);
            progress.Report(92);

            // 建造リソースを復元
            RestoreBuildResource(conn);
            progress.Report(94);

            //保管庫割当情報を読み込み
            RestoreStorageAssignInfo(conn);
            progress.Report(96);

            // ステーション設定復元
            RestoreSettings(conn, _workArea.StationData.Settings);
            progress.Report(98);

            // 各要素を未編集状態にする
            InitEditStatus();
            progress.Report(100);

            _workArea.Title = System.IO.Path.GetFileNameWithoutExtension(Path);

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            conn.Rollback();
        }
    }


    /// <summary>
    /// ステーションの設定を復元
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    protected virtual void RestoreSettings(DBConnection conn, StationSettingInfo settings)
    {
        // 本部か
        const string SQL_1 = "SELECT Value FROM StationSettings WHERE Key = 'IsHeadquarters' UNION ALL SELECT 'False' LIMIT 1";
        settings.IsHeadquarters = conn.QuerySingle<string>(SQL_1) == bool.TrueString;

        // 日光
        const string SQL_2 = "SELECT Value FROM StationSettings WHERE Key = 'Sunlight' UNION ALL SELECT '100' LIMIT 1";
        var sunLightString = conn.QuerySingle<string>(SQL_2);
        if (double.TryParse(sunLightString, out var sunLight))
        {
            settings.Sunlight = sunLight;
        }

        // 現在の労働者数
        const string SQL_3 = "SELECT Value FROM StationSettings WHERE Key = 'ActualWorkforce' UNION ALL SELECT '0' LIMIT 1";
        var actualWorkforceString = conn.QuerySingle<string>(SQL_3);
        if (long.TryParse(actualWorkforceString, out var actualWorkforce))
        {
            settings.Workforce.Actual = actualWorkforce;
        }

        // (労働者数を)常に最大にするか
        const string SQL_4 = "SELECT Value FROM StationSettings WHERE key = 'AlwaysMaximumWorkforce' UNION ALL SELECT 'False' LIMIT 1";
        settings.Workforce.AlwaysMaximum = conn.QuerySingle<string>(SQL_4) == bool.TrueString;
    }


    /// <summary>
    /// 製品価格を復元
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    protected override void RestoreProducts(DBConnection conn)
    {
        const string SQL = "SELECT WareID, Price, NoBuy, NoSell FROM Products";
        foreach (var (wareID, price, noBuy, noSell) in conn.Query<(string, long, long, long)>(SQL))
        {
            var itm = _workArea.StationData.ProductsInfo.Products.FirstOrDefault(x => x.Ware.ID == wareID);
            if (itm is not null)
            {
                itm.UnitPrice = price;
                itm.NoBuy = noBuy == 1;
                itm.NoSell = noSell == 1;
            }
        }
    }


    /// <summary>
    /// 建造リソースを復元
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    protected override void RestoreBuildResource(DBConnection conn)
    {
        const string SQL = "SELECT WareID, Price, NoBuy FROM BuildResources";
        foreach (var (wareID, price, noBuy) in conn.Query<(string, long, long)>(SQL))
        {
            var itm = _workArea.StationData.BuildResourcesInfo.BuildResources.FirstOrDefault(x => x.Ware.ID == wareID);
            if (itm is not null)
            {
                itm.UnitPrice = price;
                itm.NoBuy = noBuy == 1;
            }
        }
    }
}

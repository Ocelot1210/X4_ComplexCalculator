using Collections.Pooled;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReaders;

/// <summary>
/// 保存ファイル読み込みクラス(フォーマット0用)
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="messenger">メッセージ通知用</param>
/// <param name="WorkArea">作業エリア</param>
class SaveDataReader0(IMessenger messenger, IWorkArea WorkArea) : ISaveDataReader
{
    /// <summary>
    /// 読み込み対象ファイルパス
    /// </summary>
    public string Path { set; protected get; } = "";


    /// <summary>
    /// メッセージ通知用
    /// </summary>
    protected readonly IMessenger _messenger = messenger;


    /// <summary>
    /// 作業エリア
    /// </summary>
    protected readonly IWorkArea _workArea = WorkArea;


    /// <summary>
    /// ファイル読み込み
    /// </summary>
    /// <returns>成功したか</returns>
    public virtual bool Load(IProgress<double> progress)
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
            progress.Report(95);

            // 建造リソースを復元
            RestoreBuildResource(conn);
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
    /// モジュールを復元
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    /// <param name="progress">進捗</param>
    /// <param name="maxProgress">進捗最大</param>
    protected virtual void RestoreModules(DBConnection conn, IProgress<double> progress, int maxProgress)
    {
        // レコード数取得
        var moduleCnt = conn.QuerySingle<int>("SELECT count(*) AS Count from Modules");
        var equipmentCnt = conn.QuerySingle<int>("SELECT count(*) AS Count from Equipments");
        var records = moduleCnt + equipmentCnt;


        using var modules = new PooledList<ModulesGridItem>(moduleCnt);
        var progressCnt = 1;

        // モジュールを復元
        const string SQL_1 = "SELECT ModuleID, Count FROM Modules ORDER BY Row ASC";
        foreach (var (moduleID, count) in conn.Query<(string, long)>(SQL_1))
        {
            var module = X4Database.Instance.Ware.TryGet<IX4Module>(moduleID);
            if (module is not null)
            {
                var mod = new ModulesGridItem(_messenger, module, null, count) { EditStatus = EditStatus.Unedited };
                modules.Add(mod);
            }
            progress.Report((double)progressCnt++ / (records * maxProgress));
        }
        
        // モジュールの装備を復元
        const string SQL_2 = "SELECT Row, EquipmentID FROM Equipments";
        foreach (var (row, equipmentID) in conn.Query<(int, string)>(SQL_2))
        {
            var eqp = X4Database.Instance.Ware.TryGet<IEquipment>(equipmentID);
            if (eqp is not null)
            {
                modules[row].AddEquipment(eqp);
            }
            progress.Report((double)progressCnt++ / (records * maxProgress));
        }

        _workArea.StationData.ModulesInfo.Modules.Reset(modules);
    }


    /// <summary>
    /// 製品価格を復元
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    protected virtual void RestoreProducts(DBConnection conn)
    {
        const string SQL = "SELECT WareID, Price FROM Products";

        using var dict = conn.Query<(string, long)>(SQL).ToPooledDictionary(x => x.Item1, x => x.Item2);
        foreach (var item in _workArea.StationData.ProductsInfo.Products)
        {
            if (dict.TryGetValue(item.Ware.ID, out var value))
            {
                item.UnitPrice = value;
            }
        }
    }


    /// <summary>
    /// 建造リソースを復元
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    protected virtual void RestoreBuildResource(DBConnection conn)
    {
        const string SQL = "SELECT WareID, Price FROM BuildResources";

        using var dict = conn.Query<(string, long)>(SQL).ToPooledDictionary(x => x.Item1, x => x.Item2);
        foreach (var item in _workArea.StationData.BuildResourcesInfo.BuildResources)
        {
            if (dict.TryGetValue(item.Ware.ID, out var value))
            {
                item.UnitPrice = value;
            }
        }
    }


    /// <summary>
    /// 編集状態を初期化
    /// </summary>
    protected virtual void InitEditStatus()
    {
        // 初期化対象
        IEnumerable<IEditable>[] initTargets =
        [
            _workArea.StationData.ProductsInfo.Products,
            _workArea.StationData.BuildResourcesInfo.BuildResources,
            _workArea.StationData.StorageAssignInfo.StorageAssign,
        ];

        foreach (var editables in initTargets)
        {
            foreach (var editable in editables)
            {
                editable.EditStatus = EditStatus.Unedited;
            }
        }
    }
}

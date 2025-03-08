using Collections.Pooled;
using CommunityToolkit.Mvvm.Messaging;
using System;
using X4_ComplexCalculator.DB;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReaders;

/// <summary>
/// 保存ファイル読み込みクラス(フォーマット1用)
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="messenger">メッセージ通知用</param>
/// <param name="workArea">作業エリア</param>
class SaveDataReader1(IMessenger messenger, IWorkArea workArea) : SaveDataReader0(messenger, workArea)
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
            progress.Report(93);

            // 建造リソースを復元
            RestoreBuildResource(conn);
            progress.Report(96);

            //保管庫割当情報を読み込み
            RestoreStorageAssignInfo(conn);
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
    /// 保管庫割当情報を読み込み
    /// </summary>
    /// <param name="conn"></param>
    protected virtual void RestoreStorageAssignInfo(DBConnection conn)
    {
        const string SQL = "SELECT WareID, AllocCount FROM StorageAssign";

        using var dict = conn.Query<(string, long)>(SQL).ToPooledDictionary(x => x.Item1, x => x.Item2);
        foreach (var item in _workArea.StationData.StorageAssignInfo.StorageAssign)
        {
            if (dict.TryGetValue(item.WareID, out var value))
            {
                item.AllocCount = value;
            }
        }
    }
}

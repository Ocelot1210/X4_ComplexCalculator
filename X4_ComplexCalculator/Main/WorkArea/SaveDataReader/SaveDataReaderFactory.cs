using X4_ComplexCalculator.DB;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReader;

/// <summary>
/// 保存ファイル読み込みクラスのインスタンス作成用クラス
/// </summary>
internal static class SaveDataReaderFactory
{
    /// <summary>
    /// インスタンス作成
    /// </summary>
    /// <param name="path">読み込み対象ファイルパス</param>
    /// <param name="WorkArea">作業エリア</param>
    /// <returns>保存ファイル読み込みクラスのインスタンス</returns>
    public static ISaveDataReader CreateSaveDataReader(string path, IWorkArea WorkArea)
    {
        var version = GetVersion(path);
        ISaveDataReader ret = version switch
        {
            0 => new SaveDataReader0(WorkArea),
            1 => new SaveDataReader1(WorkArea),
            _ => new SaveDataReader2(WorkArea),
        };

        ret.Path = path;

        return ret;
    }


    /// <summary>
    /// 保存ファイルのバージョン取得
    /// </summary>
    /// <param name="path">ファイルパス</param>
    /// <returns>バージョン</returns>
    private static int GetVersion(string path)
    {
        using var conn = new DBConnection(path);

        const string sql1 = "SELECT count(*) FROM sqlite_master WHERE type = 'table' AND name = 'Common'";
        var tableExists = conn.QuerySingle<bool>(sql1);
        if (!tableExists) return 0;

        const string sql2 = "SELECT Value FROM Common WHERE Item = 'FormatVersion'";
        return conn.QuerySingle<int>(sql2);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;
using X4_ComplexCalculator.DB;

namespace X4_ComplexCalculator.Main;

/// <summary>
/// メイン画面のModel
/// </summary>
/// <param name="workAreaManager">作業エリア管理用</param>
/// <param name="localizedMessageBox">メッセージボックス表示用</param>
class MainWindowModel(WorkAreaManager workAreaManager, ILocalizedMessageBox localizedMessageBox)
{
    #region メンバ
    /// <summary>
    /// 作業エリア管理用
    /// </summary>
    private readonly WorkAreaManager _workAreaManager = workAreaManager;


    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _localizedMessageBox = localizedMessageBox;
    #endregion


    /// <summary>
    /// ウィンドウがロードされた時
    /// </summary>
    public async Task InitAsync()
    {
        // DB接続開始
        X4Database.Open(_localizedMessageBox);
        SettingDatabase.Open();

        const string SQL = "SELECT Path FROM OpenedFiles";
        var paths = SettingDatabase.Instance.Query<string>(SQL)
            .Where(x => File.Exists(x))
            .ToArray();

        await _workAreaManager.OpenFilesAsync(paths);

        // 何も開かなければ空の計画を追加する
        if (!paths.Any())
        {
            _workAreaManager.CreateNewDocument();
        }
    }


    /// <summary>
    /// DB更新
    /// </summary>
    public void UpdateDB()
    {
        switch (X4Database.UpdateDB())
        {
            // DB更新成功
            case X4Database.UpdateDbStatus.Succeeded:
                _localizedMessageBox.Ok("Lang:MainWindow_Menu_File_UpdateDB_DBUpdate_RestartRequestMessage", "Lang:Common_MessageBoxTitle_Confirmation");
                break;

            // DB更新失敗
            case X4Database.UpdateDbStatus.Failed:
                _localizedMessageBox.Error("Lang:MainWindow_Menu_File_UpdateDB_DBUpdate_FailureMessage", "Lang:Common_MessageBoxTitle_Error");
                break;

            // 更新なし
            default:
                break;
        }
    }


    /// <summary>
    /// ウィンドウが閉じられる時
    /// </summary>
    /// <returns>キャンセルされたか</returns>
    public bool WindowClosing()
    {
        var canceled = false;

        // 未保存の内容が存在するか？
        if (_workAreaManager.Documents.Any(x => x.HasChanged))
        {
            (string, string?)[] buttons = { 
                ("Lang:MainWindow_ClosingConfirmMessage_Save", null),
                ("Lang:MainWindow_ClosingConfirmMessage_DontSave", "Lang:MainWindow_ClosingConfirmMessage_DontSave_Description"),
                ("Lang:MainWindow_ClosingConfirmMessage_Cancel", "Lang:MainWindow_ClosingConfirmMessage_Cancel_Description"),
            };
            var result = _localizedMessageBox.MultiChoiceInfo("Lang:MainWindow_ClosingConfirmMessage", "Lang:Common_MessageBoxTitle_Confirmation", buttons, 2);

            switch (result)
            {
                // 保存する場合
                case 0:
                    foreach (var doc in _workAreaManager.Documents)
                    {
                        doc.Save();
                    }
                    break;

                // 保存せずに閉じる場合
                case 1:
                    break;

                // キャンセルする場合
                default:
                    canceled = true;
                    break;
            }
        }

        // 閉じる場合、開いていたファイル一覧を保存する
        if (!canceled)
        {
            var paths = _workAreaManager.Documents
                .Where(x => File.Exists(x.SaveFilePath))
                .Select(x => new { Path = x.SaveFilePath });

            SettingDatabase.Instance.BeginTransaction((con) =>
            {
                con.Execute("DELETE FROM OpenedFiles");
                con.Execute("INSERT INTO OpenedFiles(Path) VALUES(@Path)", paths);
            });
        }

        return canceled;
    }


    /// <summary>
    /// ファイル又はフォルダの一覧から保存したファイルを開く
    /// </summary>
    /// <param name="paths">ファイル又はフォルダパスの列挙</param>
    public Task OpenFilesAsync(IEnumerable<string> paths)
    {
        return _workAreaManager.OpenFilesAsync(GetX4Files(paths, 1));
    }


    /// <summary>
    /// ファイル/フォルダ内の.x4ファイルを列挙する
    /// </summary>
    /// <param name="paths">ファイル/フォルダパス</param>
    /// <param name="maxRecursion">最大再帰回数</param>
    /// <param name="currRecursion">現在の再帰回数</param>
    /// <returns></returns>
    private static IEnumerable<string> GetX4Files(IEnumerable<string> paths, int maxRecursion, int currRecursion = 0)
    {
        // 再帰最大の場合、何もしない
        if (maxRecursion < currRecursion)
        {
            yield break;
        }

        foreach (var path in paths)
        {
            // パスはフォルダか？
            if (Directory.Exists(path))
            {
                // フォルダの場合
                var files = GetX4Files(Directory.EnumerateFileSystemEntries(path), maxRecursion, currRecursion++);

                foreach (var file in files)
                {
                    yield return file;
                }
            }
            else
            {
                // ファイルの場合
                if (Path.GetExtension(path).Equals(".x4", StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return path;
                }
            }
        }

        yield break;
    }
}

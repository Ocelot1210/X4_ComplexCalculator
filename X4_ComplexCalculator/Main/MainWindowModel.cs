using System.Collections.Generic;
using System.IO;
using System.Linq;
using X4_ComplexCalculator.Common.Dialog.MessageBoxes;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main;

/// <summary>
/// メイン画面のModel
/// </summary>
class MainWindowModel
{
    #region メンバ
    /// <summary>
    /// 作業エリア管理用
    /// </summary>
    private readonly WorkAreaManager _workAreaManager;


    /// <summary>
    /// 作業エリアファイル入出力用
    /// </summary>
    private readonly WorkAreaFileIO _workAreFileIO;


    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _localizedMessageBox;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="workAreaManager">作業エリア管理用</param>
    /// <param name="workAreaFileIO">作業エリアファイル入出力用</param>
    /// <param name="localizedMessageBox">メッセージボックス表示用</param>
    public MainWindowModel(WorkAreaManager workAreaManager, WorkAreaFileIO workAreaFileIO, ILocalizedMessageBox localizedMessageBox)
    {
        _workAreaManager = workAreaManager;
        _workAreFileIO = workAreaFileIO;
        _localizedMessageBox = localizedMessageBox;
    }


    /// <summary>
    /// ウィンドウがロードされた時
    /// </summary>
    public void Init()
    {
        // DB接続開始
        X4Database.Open(_localizedMessageBox);
        SettingDatabase.Open();

        var vmList = new List<WorkAreaViewModel>();

        const string SQL = "SELECT Path FROM OpenedFiles";
        var pathes = SettingDatabase.Instance.Query<string>(SQL)
            .Where(x => File.Exists(x))
            .ToArray();

        _workAreFileIO.OpenFiles(pathes);

        // 何も開かなければ空の計画を追加する
        if (!pathes.Any())
        {
            _workAreFileIO.CreateNew();
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
}

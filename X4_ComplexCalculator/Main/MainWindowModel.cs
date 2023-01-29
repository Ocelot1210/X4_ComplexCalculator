using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using X4_ComplexCalculator.Common.Localize;
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
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public MainWindowModel(WorkAreaManager workAreaManager, WorkAreaFileIO workAreaFileIO)
    {
        _workAreaManager = workAreaManager;
        _workAreFileIO = workAreaFileIO;
    }


    /// <summary>
    /// ウィンドウがロードされた時
    /// </summary>
    public void Init()
    {
        // DB接続開始
        X4Database.Open();
        SettingDatabase.Open();

        var vmList = new List<WorkAreaViewModel>();

        const string SQL = "SELECT Path FROM OpenedFiles";
        var pathes = SettingDatabase.Instance.Query<string>(SQL)
            .Where(x => File.Exists(x))
            .ToArray();


        // 開いているファイルテーブルを初期化
        SettingDatabase.Instance.Execute("DELETE FROM OpenedFiles");

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
    public static void UpdateDB()
    {
        var result = LocalizedMessageBox.Show("Lang:MainWindow_Menu_File_UpdateDB_DBUpdate_ConfirmationMessage", "Lang:Common_MessageBoxTitle_Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            switch (X4Database.UpdateDB())
            {
                // DB更新成功
                case X4Database.UpdateDbStatus.Succeeded:
                    LocalizedMessageBox.Show("Lang:MainWindow_Menu_File_UpdateDB_DBUpdate_RestartRequestMessage", "Lang:Common_MessageBoxTitle_Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;

                // DB更新失敗
                case X4Database.UpdateDbStatus.Failed:
                    LocalizedMessageBox.Show("Lang:MainWindow_Menu_File_UpdateDB_DBUpdate_FailureMessage", "Lang:Common_MessageBoxTitle_Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

                // 更新なし
                default:
                    break;
            }
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
            var result = LocalizedMessageBox.Show("Lang:MainWindow_ClosingConfirmMessage", "Lang:Common_MessageBoxTitle_Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            switch (result)
            {
                // 保存する場合
                case MessageBoxResult.Yes:
                    foreach (var doc in _workAreaManager.Documents)
                    {
                        doc.Save();
                    }
                    break;

                // 保存せずに閉じる場合
                case MessageBoxResult.No:
                    break;

                // キャンセルする場合
                case MessageBoxResult.Cancel:
                    canceled = true;
                    break;
            }
        }

        // 閉じる場合、開いていたファイル一覧を保存する
        if (!canceled)
        {
            var pathes = _workAreaManager.Documents
                .Where(x => File.Exists(x.SaveFilePath))
                .Select(x => new { Path = x.SaveFilePath });

            SettingDatabase.Instance.Execute("INSERT INTO OpenedFiles(Path) VALUES(@Path)", pathes);
        }

        return canceled;
    }
}

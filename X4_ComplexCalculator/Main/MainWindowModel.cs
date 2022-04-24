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
    private readonly WorkAreaManager _WorkAreaManager;


    /// <summary>
    /// 作業エリアファイル入出力用
    /// </summary>
    private readonly WorkAreaFileIO _WorkAreFileIO;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public MainWindowModel(WorkAreaManager workAreaManager, WorkAreaFileIO workAreaFileIO)
    {
        _WorkAreaManager = workAreaManager;
        _WorkAreFileIO = workAreaFileIO;
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

        const string sql = "SELECT Path FROM OpenedFiles";
        var pathes = SettingDatabase.Instance.Query<string>(sql)
            .Where(x => File.Exists(x))
            .ToArray();


        // 開いているファイルテーブルを初期化
        SettingDatabase.Instance.Execute("DELETE FROM OpenedFiles");

        _WorkAreFileIO.OpenFiles(pathes);

        // 何も開かなければ空の計画を追加する
        if (!pathes.Any())
        {
            _WorkAreFileIO.CreateNew();
        }
    }


    /// <summary>
    /// DB更新
    /// </summary>
    public void UpdateDB()
    {
        var result = LocalizedMessageBox.Show("Lang:MainWindow_Menu_File_UpdateDB_DBUpdate_ConfirmationMessage", "Lang:Common_MessageBoxTitle_Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            if (X4Database.UpdateDB())
            {
                // DB更新成功
                LocalizedMessageBox.Show("Lang:MainWindow_Menu_File_UpdateDB_DBUpdate_RestartRequestMessage", "Lang:Common_MessageBoxTitle_Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // DB更新失敗
                LocalizedMessageBox.Show("Lang:MainWindow_Menu_File_UpdateDB_DBUpdate_FailureMessage", "Lang:Common_MessageBoxTitle_Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
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
        if (_WorkAreaManager.Documents.Any(x => x.HasChanged))
        {
            var result = LocalizedMessageBox.Show("Lang:MainWindow_ClosingConfirmMessage", "Lang:Common_MessageBoxTitle_Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            switch (result)
            {
                // 保存する場合
                case MessageBoxResult.Yes:
                    foreach (var doc in _WorkAreaManager.Documents)
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
            var pathes = _WorkAreaManager.Documents
                .Where(x => File.Exists(x.SaveFilePath))
                .Select(x => new { Path = x.SaveFilePath });

            SettingDatabase.Instance.Execute("INSERT INTO OpenedFiles(Path) VALUES(@Path)", pathes);
        }

        return canceled;
    }
}

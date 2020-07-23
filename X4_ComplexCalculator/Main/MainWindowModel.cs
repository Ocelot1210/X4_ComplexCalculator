using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main
{
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
            DBConnection.Open();

            var pathes = new List<string>();

            var vmList = new List<WorkAreaViewModel>();

            DBConnection.CommonDB.ExecQuery("SELECT * FROM OpenedFiles", (dr, _) =>
            {
                var path = (string)dr["Path"];
                if (File.Exists(path))
                {
                    pathes.Add(path);
                }
            });

            // 開いているファイルテーブルを初期化
            DBConnection.CommonDB.ExecQuery("DELETE FROM OpenedFiles");

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
            var result = LocalizedMessageBox.Show("Lang:DBUpdateConfirmationMessage", "Lang:Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (DBConnection.UpdateDB())
                {
                    // DB更新成功
                    LocalizedMessageBox.Show("Lang:DBUpdateRestartRequestMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // DB更新失敗
                    LocalizedMessageBox.Show("Lang:DBUpdateFailureMessage", "Lang:Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
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
            if (_WorkAreaManager.Documents.Where(x => x.HasChanged).Any())
            {
                var result = LocalizedMessageBox.Show("Lang:MainWindowClosingConfirmMessage", "Lang:Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

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
                var param = new SQLiteCommandParameters(1);

                var pathes = _WorkAreaManager.Documents.Where(x => File.Exists(x.SaveFilePath))
                                                       .Select(x => x.SaveFilePath);

                param.AddRange("path", System.Data.DbType.String, pathes);

                DBConnection.CommonDB.ExecQuery("INSERT INTO OpenedFiles(Path) VALUES(:path)", param);
            }

            return canceled;
        }
    }
}

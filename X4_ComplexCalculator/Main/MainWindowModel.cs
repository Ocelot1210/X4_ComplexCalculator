using Prism.Mvvm;
using System.Linq;
using System.Windows;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB;

namespace X4_ComplexCalculator.Main
{
    /// <summary>
    /// メイン画面のModel
    /// </summary>
    class MainWindowModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 作業エリア管理用
        /// </summary>
        private readonly WorkAreaManager _WorkAreaManager;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowModel(WorkAreaManager workAreaManager)
        {
            _WorkAreaManager = workAreaManager;
        }


        /// <summary>
        /// ウィンドウがロードされた時
        /// </summary>
        public void Init()
        {
            // DB接続開始
            DBConnection.Open();
        }



        /// <summary>
        /// DB更新
        /// </summary>
        public void UpdateDB()
        {
            var result = Localize.ShowMessageBox("Lang:DBUpdateConfirmationMessage", "Lang:Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (!DBConnection.UpdateDB())
                {
                    Localize.ShowMessageBox("Lang:DBUpdateConfirmationMessage", "Lang:Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Error);
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
                var result = Localize.ShowMessageBox("Lang:MainWindowClosingConfirmMessage", "Lang:Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

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

            // 閉じる場合、リソースを開放
            if (!canceled)
            {
                foreach (var doc in _WorkAreaManager.Documents)
                {
                    doc.Dispose();
                }
            }

            return canceled;
        }
    }
}

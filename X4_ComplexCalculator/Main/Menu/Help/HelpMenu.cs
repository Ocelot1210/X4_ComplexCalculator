using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;
using X4_ComplexCalculator.Infrastructures;

namespace X4_ComplexCalculator.Main.Menu.Help
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="localizedMessageBox">メッセージボックス表示用</param>
    internal partial class HelpMenu(ILocalizedMessageBox localizedMessageBox) : IDisposable
    {
        /// <summary>
        /// メッセージボックス表示用
        /// </summary>
        private readonly ILocalizedMessageBox _localizedMessageBox = localizedMessageBox;

        /// <summary>
        /// アップデート機能
        /// </summary>
        private readonly ApplicationUpdater _applicationUpdater = new();


        /// <summary>
        /// バージョン情報
        /// </summary>
        public void ShowVersionInfo()
        {
            const string VERSION = VersionInfo.DETAIL_VERSION;
            const string COMMIT = ThisAssembly.Git.Sha;
            const string DATE = ThisAssembly.Git.CommitDate;
            var dotnetVersion = Environment.Version.ToString();

            _localizedMessageBox.Ok("Lang:MainWindow_Menu_Help_VersionInfo_MessageDescription", "Lang:MainWindow_Menu_Help_VersionInfo_MessageTitle", VERSION, COMMIT, DATE, dotnetVersion);
        }

        /// <summary>
        /// 問題を報告
        /// </summary>
        public void ReportIssue()
        {
            string url = ThisAssembly.Git.RepositoryUrl + "/issues";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }


        /// <summary>
        /// 更新確認ON/OFF
        /// </summary>
        public void SetCheckUpdateAtLaunch()
        {
            Configuration.Instance.CheckUpdateAtLaunch = !Configuration.Instance.CheckUpdateAtLaunch;
        }


        /// <summary>
        /// 更新を確認...
        /// </summary>
        public async Task CheckUpdateAsync(bool isUserOperation = false)
        {
            if (_applicationUpdater.FinishedDownload && isUserOperation)
            {
                _localizedMessageBox.Ok("Lang:CheckUpdate_FinishedDownloadDescription", "Lang:CheckUpdate_Title");
                return;
            }
            else if (_applicationUpdater.NowDownloading && isUserOperation)
            {
                _localizedMessageBox.Ok("Lang:CheckUpdate_StartDownloadDescription", "Lang:CheckUpdate_Title");
                return;
            }

            string? latestVersion;
            try
            {
                latestVersion = await _applicationUpdater.CheckUpdate();
            }
            catch (HttpRequestException)
            {
                if (isUserOperation)
                {
                    _localizedMessageBox.Error("Lang:CheckUpdate_FailedDescription", "Lang:CheckUpdate_Title");
                }
                return;
            }
            if (latestVersion is null)
            {
                if (isUserOperation)
                {
                    _localizedMessageBox.Ok("Lang:CheckUpdate_NoUpdateDescription", "Lang:CheckUpdate_Title", VersionInfo.BASE_VERSION);
                }
                return;
            }

            var result = _localizedMessageBox.YesNo("Lang:CheckUpdate_HasUpdateDescription", "Lang:CheckUpdate_Title", LocalizedMessageBoxResult.Yes, VersionInfo.BASE_VERSION, latestVersion);

            if (result != LocalizedMessageBoxResult.Yes) return;

            _applicationUpdater.StartDownloadByBackground();
            _localizedMessageBox.Ok("Lang:CheckUpdate_StartDownloadDescription", "Lang:CheckUpdate_Title");
        }


        public void Dispose()
        {
            if (_applicationUpdater.FinishedDownload)
            {
                _applicationUpdater.Update();
            }
            else if (_applicationUpdater.NowDownloading)
            {
                var dialog = new UpdateDownloadProglessDialog
                {
                    DataContext = new UpdateDownloadProgressViewModel(_applicationUpdater)
                };
                dialog.Show();
            }
        }
    }
}

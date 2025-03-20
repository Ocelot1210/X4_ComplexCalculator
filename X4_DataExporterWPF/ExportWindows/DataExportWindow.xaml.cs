using CommunityToolkit.Mvvm.Messaging;
using LibX4.FileSystem;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using WPFLocalizeExtension.Engine;
using X4_DataExporterWPF.Export;
using X4_DataExporterWPF.ExportWindows.DependencyResolutionFailedWindows;

namespace X4_DataExporterWPF.ExportWindows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class DataExportWindow : Window
{
    private readonly IMessenger _messenger;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="inDirPath">入力元フォルダパス</param>
    /// <param name="outFilePath">出力先ファイルパス</param>
    private DataExportWindow(string inDirPath, string outFilePath)
    {
        InitializeComponent();

        _messenger = new WeakReferenceMessenger();
        _messenger.Register<DependencyResolutionException>(this, OnDependencyResolutionFailed);
        _messenger.Register<DbBackupException>(this, OnDbBackupFailed);
        _messenger.Register<Tuple<Exception, string>>(this, OnExportFailed);
        _messenger.Register<Tuple<string, string>>(this, OnExportSuccess);

        DataContext = new DataExportViewModel(_messenger, inDirPath, outFilePath);
    }


    /// <summary>
    /// ダイアログを表示
    /// </summary>
    /// <param name="inDirPath"></param>
    /// <param name="outFilePath"></param>
    public static void ShowDialog(string inDirPath, string outFilePath)
    {
        var wnd = new DataExportWindow(inDirPath, outFilePath)
        {
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive) ?? Application.Current.MainWindow
        };

        wnd.ShowDialog();
    }


    /// <summary>
    /// DB 抽出成功時
    /// </summary>
    private void OnExportSuccess(object recipient, Tuple<string, string> message)
    {
        Dispatcher.Invoke(() =>
        {
            MessageBox.Show(
                (string)LocalizeDictionary.Instance.GetLocalizedObject(message.Item1, null, null),
                (string)LocalizeDictionary.Instance.GetLocalizedObject(message.Item2, null, null),
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        });
    }


    /// <summary>
    /// DB 抽出失敗時
    /// </summary>
    private void OnExportFailed(object recipient, Tuple<Exception, string> message)
    {
        Dispatcher.Invoke(() =>
        {
            MessageBox.Show(
                (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:DataExporter_FailedToExportMessage", null, null),
                (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:DataExporter_Title", null, null),
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );

            Process.Start("explorer.exe", $@"/select,""{message.Item2}""");
        });
    }


    /// <summary>
    /// DB のバックアップに失敗時
    /// </summary>
    private void OnDbBackupFailed(object recipient, DbBackupException message)
    {
        Dispatcher.Invoke(() =>
        {
            MessageBox.Show(
                (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:DataExporter_FailedToBackupDb", null, null),
                (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:DataExporter_Title", null, null),
                MessageBoxButton.OK, 
                MessageBoxImage.Error
            );
        });
    }


    /// <summary>
    /// 依存関係の解決に失敗時
    /// </summary>
    private void OnDependencyResolutionFailed(object recipient, DependencyResolutionException message)
    {
        Dispatcher.Invoke(() =>
        {
            var wnd = new DependencyResolutionFailedWindow(message.UnloadedMods);
            wnd.Owner = this;
            wnd.ShowDialog();
        });
    }
}

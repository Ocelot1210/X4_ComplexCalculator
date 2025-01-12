using Collections.Pooled;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Dialog.MessageBoxes;

namespace X4_ComplexCalculator.Main.Menu.File.Importers.SaveDataImporters;

/// <summary>
/// コンストラクタ
/// </summary>
/// <param name="messageBox">メッセージボックス表示用</param>
partial class SelectStationModel(List<SaveDataStationItem> stationItems, ILocalizedMessageBox messageBox) : ObservableObject
{
    #region メンバ
    /// <summary>
    /// 選択された計画一覧
    /// </summary>
    private readonly List<SaveDataStationItem> _selectedStationItems = stationItems;


    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _messageBox = messageBox;
    #endregion


    #region プロパティ
    /// <summary>
    /// 計画一覧
    /// </summary>
    public ObservableRangeCollection<SaveDataStationItem> Stations { get; } = new ObservableRangeCollection<SaveDataStationItem>();


    /// <summary>
    /// 計画ファイルパス
    /// </summary>
    [ObservableProperty]
    public partial string SaveDataFilePath { get; set; } = "";
    #endregion


    /// <summary>
    /// 初期フォルダを取得する
    /// </summary>
    /// <returns></returns>
    private static string GetInitialDirectory()
    {
        var dirs = LibX4.X4Path.EnumerateConfigFolders().ToArray();
        if (dirs.Any())
        {
            return Path.Combine(dirs.First(), "save");
        }

        return "";
    }


    /// <summary>
    /// セーブデータを選択
    /// </summary>
    public void SelectSaveDataFile()
    {
        var dlg = new OpenFileDialog
        {
            InitialDirectory = GetInitialDirectory(),
            RestoreDirectory = true,
            Filter = "X4 Save data file (*.xml;*.xml.gz)|*.xml;*.xml.gz|All files (*.*)|*.*"
        };
        if (dlg.ShowDialog() == true)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                switch (Path.GetExtension(dlg.FileName))
                {
                    case ".xml":
                        XmlRead(dlg.FileName);
                        break;

                    case ".gz":
                        GzRead(dlg.FileName);
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                SaveDataFilePath = dlg.FileName;

            }
            catch (Exception e)
            {
                _messageBox.Error("Lang:MainWindow_FaildToLoadFileMessage", "Lang:MainWindow_FaildToLoadFileMessageTitle", e.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }


    /// <summary>
    /// 選択確定
    /// </summary>
    public void OnOk()
    {
        _selectedStationItems.AddRange(Stations.Where(x => x.IsChecked));
    }


    /// <summary>
    /// 非圧縮形式のファイル読み込み
    /// </summary>
    /// <param name="path"></param>
    private void XmlRead(string path)
    {
        using var sr = new FileStream(path, FileMode.Open, FileAccess.Read);

        var xmlReader = XmlReader.Create(sr);

        SaveDataFileReadMain(xmlReader);
    }


    /// <summary>
    /// 圧縮形式のファイル読み込み
    /// </summary>
    /// <param name="path"></param>
    private void GzRead(string path)
    {
        using var sr = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var gz = new GZipStream(sr, CompressionMode.Decompress, true);

        var xmlReader = XmlReader.Create(gz);

        SaveDataFileReadMain(xmlReader);
    }


    /// <summary>
    /// ファイル読み込みメイン処理
    /// </summary>
    /// <param name="reader"></param>
    private void SaveDataFileReadMain(XmlReader reader)
    {
        using var addItems = new PooledList<SaveDataStationItem>();

        while (reader.Read())
        {
            if (!(reader.Name == "component" && reader.GetAttribute("class") == "station" && reader.GetAttribute("owner") == "player" && !string.IsNullOrEmpty(reader.GetAttribute("name"))))
            {
                continue;
            }

            addItems.Add(new SaveDataStationItem("", XElement.Parse(reader.ReadOuterXml())));
        }

        Stations.Reset(addItems);
    }
}

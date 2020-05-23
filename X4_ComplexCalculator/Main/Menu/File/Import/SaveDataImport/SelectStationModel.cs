using Microsoft.Win32;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Localize;

namespace X4_ComplexCalculator.Main.Menu.File.Import.SaveDataImport
{
    class SelectStationModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// セーブデータファイルパス
        /// </summary>
        string _SaveDataFilePath;
        #endregion

        /// <summary>
        /// 計画一覧
        /// </summary>
        public ObservableRangeCollection<SaveDataStationItem> Stations { get; } = new ObservableRangeCollection<SaveDataStationItem>();

        /// <summary>
        /// 計画ファイルパス
        /// </summary>
        public string SaveDataFilePath
        {
            get => _SaveDataFilePath;
            set => SetProperty(ref _SaveDataFilePath, value);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SelectStationModel()
        {

        }

        /// <summary>
        /// 初期フォルダを取得する
        /// </summary>
        /// <returns></returns>
        private string GetInitialDirectory()
        {
            var docDir = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            var x4Dir = Path.Combine(docDir, "Egosoft\\X4");

            var dirs = Directory.GetDirectories(x4Dir);
            if (dirs.Any())
            {
                return Path.Combine(dirs.First(), "save");
            }

            return x4Dir;
        }


        /// <summary>
        /// セーブデータを選択
        /// </summary>
        public void SelectSaveDataFile()
        {
            var dlg = new OpenFileDialog();

            dlg.InitialDirectory = GetInitialDirectory();
            dlg.RestoreDirectory = true;
            dlg.Filter = "X4 Save data file (*.xml;*.xml.gz)|*.xml;*.xml.gz|All files (*.*)|*.*";
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
                    Dispatcher.CurrentDispatcher.BeginInvoke(() =>
                    {
                        Localize.ShowMessageBox("Lang:FaildToLoadFileMessage", "Lang:FaildToLoadFileMessageTitle", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, e.Message);
                    });
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
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
            var addItems = new List<SaveDataStationItem>();

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
}

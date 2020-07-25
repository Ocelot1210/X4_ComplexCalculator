using Microsoft.Win32;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Localize;

namespace X4_ComplexCalculator.Main.Menu.File.Import.LoadoutImport
{
    class SelectLoadoutModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 装備プリセットファイルパス
        /// </summary>
        string _LoadoutsFilePath = "";
        #endregion

        /// <summary>
        /// 計画一覧
        /// </summary>
        public ObservableRangeCollection<LoadoutItem> Loadouts { get; } = new ObservableRangeCollection<LoadoutItem>();


        /// <summary>
        /// 装備プリセットファイルパス
        /// </summary>
        public string LoadoutsFilePath
        {
            get => _LoadoutsFilePath;
            set => SetProperty(ref _LoadoutsFilePath, value);
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SelectLoadoutModel()
        {
            var docDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            var x4Dir = Path.Combine(docDir, "Egosoft\\X4");

            var dirs = Directory.GetDirectories(x4Dir);
            if (dirs.Any())
            {
                LoadoutsFilePath = Path.Combine(dirs.First(), "loadouts.xml");
            }

            try
            {
                Read(LoadoutsFilePath);
            }
            catch
            {

            }
        }


        /// <summary>
        /// 初期フォルダを取得する
        /// </summary>
        /// <returns></returns>
        private string GetInitialDirectory()
        {
            var docDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

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
            dlg.Filter = "X4 Loadouts data file (loadouts.xml)|loadouts.xml|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    Read(dlg.FileName);
                    LoadoutsFilePath = dlg.FileName;
                }
                catch (Exception e)
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(() =>
                    {
                        LocalizedMessageBox.Show("Lang:FaildToLoadFileMessage", "Lang:FaildToLoadFileMessageTitle", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, e.Message);
                    });
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }


        /// <summary>
        /// インポート実行
        /// </summary>
        public void Import()
        {
            var cnt = 0;
            var succeeded = 0;
            foreach (var loadout in Loadouts.Where(x => x.IsChecked))
            {
                if (loadout.Import())
                {
                    succeeded++;
                }
                cnt++;
            }

            // プリセットが何も選択されなかったか？
            if (cnt == 0)
            {
                LocalizedMessageBox.Show("Lang:NoPresetSelectedMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // プリセットの全件インポート成功したか？
            if (cnt == succeeded)
            {
                LocalizedMessageBox.Show("Lang:PresetImportSucceededMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, succeeded);
                return;
            }

            // 1件以上のインポートに失敗
            LocalizedMessageBox.Show("Lang:PresetImportSucceededMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, succeeded, cnt - succeeded);
        }


        /// <summary>
        /// ファイル読み込みメイン処理
        /// </summary>
        /// <param name="path">読み込み対象ファイルパス</param>
        private void Read(string path)
        {
            using var sr = new StreamReader(path);
            var reader = XmlReader.Create(sr);

            var addItems = new List<LoadoutItem>();

            while (reader.Read())
            {
                if (reader.Name != "loadout")
                {
                    continue;
                }

                var itm = LoadoutItem.Create(XElement.Parse(reader.ReadOuterXml()));
                if (itm != null)
                {
                    addItems.Add(itm);
                }
            }

            Loadouts.Reset(addItems);
        }
    }
}

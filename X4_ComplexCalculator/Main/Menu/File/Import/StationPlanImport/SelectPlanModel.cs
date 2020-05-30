using Microsoft.Win32;
using Prism.Mvvm;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Localize;

namespace X4_ComplexCalculator.Main.Menu.File.Import.StationPlanImport
{
    class SelectPlanModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 計画ファイルパス
        /// </summary>
        string _PlanFilePath = "";
        #endregion

        /// <summary>
        /// 計画一覧
        /// </summary>
        public ObservableRangeCollection<StationPlanItem> Planes { get; } = new ObservableRangeCollection<StationPlanItem>();

        /// <summary>
        /// 計画ファイルパス
        /// </summary>
        public string PlanFilePath
        {
            get => _PlanFilePath;
            set => SetProperty(ref _PlanFilePath, value);
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SelectPlanModel()
        {
            // ファイルが見つかった場合、そのファイルで初期化する
            var path = $"{GetInitialDirectory()}\\constructionplans.xml";
            if (System.IO.File.Exists(path))
            {
                try
                {
                    var xml = XDocument.Load(path);

                    Planes.Reset(xml.Root.XPathSelectElements("plan").Select(x => new StationPlanItem(x)));
                    PlanFilePath = path;
                }
                catch
                {

                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        /// <summary>
        /// 初期フォルダを取得する
        /// </summary>
        /// <returns></returns>
        private string GetInitialDirectory()
        {
            var docDir = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            var x4Dir = Path.Combine(docDir, "Egosoft\\X4");

            return Directory.GetDirectories(x4Dir).FirstOrDefault() ?? x4Dir;
        }


        /// <summary>
        /// 建造計画ファイルを選択
        /// </summary>
        public void SelectPlanFile()
        {
            var dlg = new OpenFileDialog();

            dlg.InitialDirectory = GetInitialDirectory();
            dlg.RestoreDirectory = true;
            dlg.Filter = "X4 Construction planes file (constructionplans.xml)|constructionplans.xml";
            if (dlg.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    var xml = XDocument.Load(dlg.FileName);

                    Planes.Reset(xml.Root.XPathSelectElements("plan").Select(x => new StationPlanItem(x)));
                    PlanFilePath = dlg.FileName;
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
    }
}

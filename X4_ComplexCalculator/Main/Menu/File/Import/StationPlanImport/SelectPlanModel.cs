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

namespace X4_ComplexCalculator.Main.Menu.File.Import.StationPlanImport;

class SelectPlanModel : BindableBase
{
    #region メンバ
    /// <summary>
    /// 計画ファイルパス
    /// </summary>
    string _PlanFilePath = "";
    #endregion


    #region プロパティ
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
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public SelectPlanModel()
    {
        // ファイルが見つかった場合、そのファイルで初期化する
        var path = Path.Combine(LibX4.X4Path.GetUserDirectory(), "constructionplans.xml");
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
    /// 建造計画ファイルを選択
    /// </summary>
    public void SelectPlanFile()
    {
        var dlg = new OpenFileDialog();

        dlg.InitialDirectory = LibX4.X4Path.GetUserDirectory();
        dlg.RestoreDirectory = true;
        dlg.Filter = "X4 Construction planes file (*.xml)|*.xml";
        dlg.Multiselect = true;

        if (dlg.ShowDialog() == true)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                Planes.Clear();

                foreach (var fileName in dlg.FileNames)
                {
                    var xml = XDocument.Load(fileName);

                    Planes.AddRange(xml.Root.XPathSelectElements("plan").Select(x => new StationPlanItem(x)));
                }

                // 複数選択されたら親フォルダパスを表示
                // 1つだけ選択されたらそのファイルパスを表示
                PlanFilePath = (1 < dlg.FileNames.Length) ? Path.GetDirectoryName(dlg.FileName) ?? "" : dlg.FileName;

            }
            catch (Exception e)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(() =>
                {
                    LocalizedMessageBox.Show("Lang:MainWindow_FaildToLoadFileMessage", "Lang:MainWindow_FaildToLoadFileMessageTitle", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, e.Message);
                });
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }
}

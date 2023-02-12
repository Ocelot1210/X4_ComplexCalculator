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
using X4_ComplexCalculator.Common.Dialog.MessageBoxes;
using X4_ComplexCalculator.Common.Localize;

namespace X4_ComplexCalculator.Main.Menu.File.Import.StationPlanImport;

class SelectPlanModel : BindableBase
{
    #region メンバ
    /// <summary>
    /// 計画ファイルパス
    /// </summary>
    string _planFilePath = "";


    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _messageBox;
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
        get => _planFilePath;
        set => SetProperty(ref _planFilePath, value);
    }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messageBox">メッセージボックス表示用</param>
    public SelectPlanModel(ILocalizedMessageBox messageBox)
    {
        _messageBox = messageBox;

        // ファイルが見つかった場合、そのファイルで初期化する
        var path = Path.Combine(LibX4.X4Path.GetUserDirectory(), "constructionplans.xml");
        if (System.IO.File.Exists(path))
        {
            try
            {
                var xml = XDocument.Load(path);
                if (xml.Root is null) return;

                var plans = xml.Root.XPathSelectElements("plan")
                    .Select(x => (ID: x.Attribute("id")?.Value, Name: x.Attribute("name")?.Value ?? "", Element: x))
                    .Where(x => !string.IsNullOrEmpty(x.ID))
                    .Select(x => new StationPlanItem(x.ID!, x.Name, x.Element));

                Planes.Reset(plans);
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
        var dlg = new OpenFileDialog
        {
            InitialDirectory = LibX4.X4Path.GetUserDirectory(),
            RestoreDirectory = true,
            Filter = "X4 Construction planes file (*.xml)|*.xml",
            Multiselect = true
        };

        if (dlg.ShowDialog() == true)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                Planes.Clear();

                foreach (var fileName in dlg.FileNames)
                {
                    var xml = XDocument.Load(fileName);
                    if (xml.Root is null) return;

                    var plans = xml.Root.XPathSelectElements("plan")
                        .Select(x => (ID: x.Attribute("id")?.Value, Name: x.Attribute("name")?.Value ?? "", Element: x))
                        .Where(x => !string.IsNullOrEmpty(x.ID))
                        .Select(x => new StationPlanItem(x.ID!, x.Name, x.Element));

                    Planes.AddRange(plans);
                }

                // 複数選択されたら親フォルダパスを表示
                // 1つだけ選択されたらそのファイルパスを表示
                PlanFilePath = (1 < dlg.FileNames.Length) ? Path.GetDirectoryName(dlg.FileName) ?? "" : dlg.FileName;

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
}

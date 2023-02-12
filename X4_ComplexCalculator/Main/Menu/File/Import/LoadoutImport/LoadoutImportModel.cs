using Microsoft.Win32;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Dialog.MessageBoxes;

namespace X4_ComplexCalculator.Main.Menu.File.Import.LoadoutImport;

/// <summary>
/// モジュール装備インポート画面のModel
/// </summary>
class LoadoutImportModel : BindableBase
{
    #region メンバ
    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _localizedMessageBox;


    /// <summary>
    /// 装備プリセットファイルパス
    /// </summary>
    string _loadoutsFilePath = "";
    #endregion


    #region プロパティ
    /// <summary>
    /// 計画一覧
    /// </summary>
    public ObservableRangeCollection<LoadoutItem> Loadouts { get; } = new();


    /// <summary>
    /// 装備プリセットファイルパス
    /// </summary>
    public string LoadoutsFilePath
    {
        get => _loadoutsFilePath;
        set => SetProperty(ref _loadoutsFilePath, value);
    }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="localizedMessageBox">メッセージボックス表示用</param>
    public LoadoutImportModel(ILocalizedMessageBox localizedMessageBox)
    {
        _localizedMessageBox = localizedMessageBox;

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
    private static string GetInitialDirectory()
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
        var dlg = new OpenFileDialog
        {
            InitialDirectory = GetInitialDirectory(),
            RestoreDirectory = true,
            Filter = "X4 Loadouts data file (loadouts.xml)|loadouts.xml|All files (*.*)|*.*"
        };
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
                _localizedMessageBox.Error("Lang:MainWindow_FaildToLoadFileMessage", "Lang:MainWindow_FaildToLoadFileMessageTitle", e.Message);
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
            _localizedMessageBox.Warn("Lang:ImportLoadout_NoPresetSelectedMessage", "Lang:Common_MessageBoxTitle_Confirmation");
            return;
        }

        // プリセットの全件インポート成功したか？
        if (cnt == succeeded)
        {
            _localizedMessageBox.Ok("Lang:ImportLoadout_ImportSucceededMessage", "Lang:Common_MessageBoxTitle_Confirmation", succeeded);
            return;
        }

        // 1件以上のインポートに失敗
        _localizedMessageBox.Error("Lang:ImportLoadout_ImportFailedMessage", "Lang:Common_MessageBoxTitle_Error", succeeded, cnt - succeeded);
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
            if (itm is not null)
            {
                addItems.Add(itm);
            }
        }

        Loadouts.Reset(addItems);
    }
}

using Collections.Pooled;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;
using X4_ComplexCalculator.Common.Dialogs.SelectStringDialog;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.Menu.File.Importers.StationCalculatorImporters;

/// <summary>
/// Station Calculatorからインポートする
/// </summary>
partial class StationCalculatorImporter : ObservableObject, IImporter
{
    #region メンバ
    /// <summary>
    /// 作業エリア管理
    /// </summary>
    private readonly WorkAreaManager _workAreaManager;


    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _localizedMessageBox;
    #endregion


    #region プロパティ
    /// <summary>
    /// メニュー表示用タイトル
    /// </summary>
    public string Title => "Lang:MainWindow_Menu_File_MenuItem_Import_MenuItem_StationCalculator_Header";
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="workAreaManager">作業エリア管理用</param>
    /// <param name="localizedMessageBox">メッセージボックス表示用</param>
    public StationCalculatorImporter(WorkAreaManager workAreaManager, ILocalizedMessageBox localizedMessageBox)
    {
        _workAreaManager = workAreaManager;
        _localizedMessageBox = localizedMessageBox;
    }


    /// <summary>
    /// インポート処理
    /// </summary>
    /// <param name="WorkArea"></param>
    [RelayCommand]
    private void Import()
    {
        var (onOK, url) = SelectStringDialog.ShowDialog("Lang:StationCalculatorImport_Title", "Lang:StationCalculatorImport_Description");
        if (!onOK)
        {
            return;
        }

        var messenger = new WeakReferenceMessenger();
        var vm = new WorkAreaViewModel(messenger, _workAreaManager.ActiveLayoutID, _localizedMessageBox.Clone());

        if (ImportMain(messenger, vm.WorkArea, url))
        {
            _workAreaManager.Documents.Add(vm);
        }
        else
        {
            vm.Dispose();
        }
    }


    /// <summary>
    /// インポート実行
    /// </summary>
    /// <param name="messenger">メッセージ通知用</param>
    /// <param name="WorkArea">作業エリア</param>
    /// <returns>インポートに成功したか</returns>
    private bool ImportMain(IMessenger messenger, IWorkArea workArea, string url)
    {
        var ret = false;

        try
        {
            var query = url.Split('?').Last();

            using var paramDict = new PooledDictionary<string, string>();


            var paramParser = new Regex(@"(\w+)=(.*)");
            foreach (var param in query.Split('&'))
            {
                var m = paramParser.Match(param);
                paramDict.Add(m.Groups[1].Value, m.Groups[2].Value);
            }

            var moduleParser = new Regex(@"\$module-(.*?),count:(.*)");
            var modules = paramDict["l"].Split(";,")
                .Select(x => moduleParser.Match(x))
                .Select(x => (Module: X4Database.Instance.Ware.TryGet<IX4Module>(x.Groups[1].Value), Count: long.Parse(x.Groups[2].Value)))
                .Where(x => x.Module is not null)
                .Select(x => (Module: x.Module!, x.Count))
                .Select(x => new ModulesGridItem(messenger, x.Module, null, x.Count, [], EditStatus.Unedited));


            workArea.StationData.ModulesInfo.Modules.AddRange(modules);
            // 編集状態を全て未編集にする
            IEnumerable<IEditable>[] editables =
            {
                workArea.StationData.ProductsInfo.Products,
                workArea.StationData.BuildResourcesInfo.BuildResources,
                workArea.StationData.StorageAssignInfo.StorageAssign,
            };
            foreach (var editable in editables.SelectMany(x => x))
            {
                editable.EditStatus = EditStatus.Unedited;
            }

            ret = true;
        }
        catch (Exception e)
        {
            _localizedMessageBox.Error("Lang:MainWindow_ImportFailureMessage", "Lang:Common_MessageBoxTitle_Error", e.Message);
        }

        return ret;
    }
}

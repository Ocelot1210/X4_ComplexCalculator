using Collections.Pooled;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.Menu.File.Importers.StationPlanImporters;

/// <summary>
/// 既存の計画ファイルからインポートする
/// </summary>
partial class StationPlanImporter : ObservableObject, IImporter
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
    public string Title => "Lang:MainWindow_Menu_File_MenuItem_Import_MenuItem_ExistingPlan_Header";
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="workAreaManager">作業エリア管理用</param>
    /// <param name="localizedMessageBox">メッセージボックス表示用</param>
    public StationPlanImporter(WorkAreaManager workAreaManager, ILocalizedMessageBox localizedMessageBox)
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
        var stations = new List<StationPlanItem>();
        if (!SelectPlanDialog.ShowDialog(stations))
        {
            return;
        }

        foreach (var station in stations)
        {
            var messenger = new WeakReferenceMessenger();
            var vm = new WorkAreaViewModel(messenger, _workAreaManager.ActiveLayoutID, _localizedMessageBox.Clone());

            if (ImportMain(messenger, vm.WorkArea, station))
            {
                _workAreaManager.Documents.Add(vm);
            }
            else
            {
                vm.Dispose();
            }
        }
    }


    /// <summary>
    /// インポートメイン処理
    /// </summary>
    /// <param name="messenger"></param>
    /// <param name="workArea"></param>
    /// <param name="planItem"></param>
    /// <returns></returns>
    private static bool ImportMain(IMessenger messenger, IWorkArea workArea, StationPlanItem planItem)
    {
        using var modules = new PooledList<ModulesGridItem>((int)(double)planItem.Plan.XPathEvaluate("count(entry)"));

        foreach (var entry in planItem.Plan.XPathSelectElements("entry"))
        {
            // マクロ名を取得
            var macro = entry.Attribute("macro")?.Value ?? "";
            if (string.IsNullOrEmpty(macro))
            {
                continue;
            }

            // マクロ名からモジュールを取得
            var module = X4Database.Instance.Ware.TryGetMacro<IX4Module>(macro);
            if (module is null)
            {
                continue;
            }

            // 本部モジュールなら本部にチェック入
            if (module.ID == "module_player_prod_hq_01_macro")
            {
                workArea.StationData.Settings.IsHeadquarters = true;
            }

            // 製造不可なモジュールはインポートしない
            if (module.Productions.Count == 0)
            {
                continue;
            }

            // モジュールの装備を取得
            var equipments = entry.XPathSelectElements("upgrades/groups/*")
                .Select(x => (Macro: x.Attribute("macro")?.Value ?? "", Count: int.Parse(x.Attribute("exact")?.Value ?? "1")))
                .Where(x => !string.IsNullOrEmpty(x.Macro))
                .Select(x => (Equipment: X4Database.Instance.Ware.TryGetMacro<IEquipment>(x.Macro), x.Count))
                .Where(x => x.Equipment is not null)
                .Select(x => (Equipment: x.Equipment!, x.Count));

            var modulesGridItem = new ModulesGridItem(messenger, module);
            foreach (var (equipment, count) in equipments)
            {
                modulesGridItem.Equipments.Add(equipment, count);
            }

            modules.Add(modulesGridItem);
        }




        // 同一モジュールをマージ
        using var dict = new PooledDictionary<int, ModulesGridItem>();

        foreach (var (module, idx) in modules.Select((x, idx) => (x, idx)))
        {
            var hash = module.GetHashCode();
            if (dict.ContainsKey(hash))
            {
                dict[hash].ModuleCount += module.ModuleCount;
            }
            else
            {
                dict.Add(hash, module);
            }
        }

        // モジュール一覧に追加
        workArea.StationData.ModulesInfo.Modules.AddRange(dict.Select(x => x.Value).OrderBy(x => x.Module.Name));

        // 編集状態を全て未編集にする
        IEnumerable<IEditable>[] editables =
        {
            workArea.StationData.ModulesInfo.Modules,
            workArea.StationData.ProductsInfo.Products,
            workArea.StationData.BuildResourcesInfo.BuildResources,
            workArea.StationData.StorageAssignInfo.StorageAssign,
        };
        foreach (var editable in editables.SelectMany(x => x))
        {
            editable.EditStatus = EditStatus.Unedited;
        }

        workArea.Title = planItem.PlanName;
        return true;
    }
}

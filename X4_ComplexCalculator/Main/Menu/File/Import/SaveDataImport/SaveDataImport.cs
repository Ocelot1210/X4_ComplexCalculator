using Collections.Pooled;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using X4_ComplexCalculator.Common.Dialog.MessageBoxes;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.Menu.File.Import.SaveDataImport;

/// <summary>
/// X4のセーブデータからインポートする機能用クラス
/// </summary>
partial class SaveDataImport : ObservableObject, IImport
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


    /// <summary>
    /// メニュー表示用タイトル
    /// </summary>
    public string Title => "X4 セーブデータ";


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="workAreaManager">作業エリア管理用</param>
    /// <param name="localizedMessageBox">メッセージボックス表示用</param>
    public SaveDataImport(WorkAreaManager workAreaManager, ILocalizedMessageBox localizedMessageBox)
    {
        _workAreaManager = workAreaManager;
        _localizedMessageBox = localizedMessageBox;
    }


    /// <summary>
    /// インポート実行
    /// </summary>
    [RelayCommand]
    private void Import()
    {
        var stations = new List<SaveDataStationItem>();
        if (!SelectStationDialog.ShowDialog(stations))
        {
            return;
        }


        foreach (var station in stations)
        {
            var vm = new WorkAreaViewModel(_workAreaManager.ActiveLayoutID, _localizedMessageBox.Clone());

            if (ImportMain(vm.WorkArea, station))
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
    /// インポート実行メイン
    /// </summary>
    /// <param name="WorkArea"></param>
    /// <param name="saveData"></param>
    /// <returns></returns>
    private static bool ImportMain(IWorkArea WorkArea, SaveDataStationItem saveData)
    {
        try
        {
            // モジュール一覧を設定
            SetModules(WorkArea, saveData);

            // 製品価格を設定
            SetWarePrice(WorkArea, saveData);

            // 保管庫割当状態を設定
            SetStorageAssign(WorkArea, saveData);

            WorkArea.Title = saveData.StationName;

            return true;
        }
        catch
        {
            return false;
        }
    }


    /// <summary>
    /// モジュール一覧を設定
    /// </summary>
    /// <param name="WorkArea"></param>
    /// <param name="saveData"></param>
    private static void SetModules(IWorkArea WorkArea, SaveDataStationItem saveData)
    {
        var modules = new List<ModulesGridItem>((int)(double)saveData.XElement.XPathEvaluate("count(construction/sequence/entry)"));

        foreach (var entry in saveData.XElement.XPathSelectElements("construction/sequence/entry"))
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

            // モジュールの装備を取得
            var equipments = entry.XPathSelectElements("upgrades/groups/*")
                .Select(x => (Macro: x.Attribute("macro")?.Value ?? "", Count: int.Parse(x.Attribute("exact")?.Value ?? "1")))
                .Where(x => !string.IsNullOrEmpty(x.Macro))
                .Select(x => (Equipment: X4Database.Instance.Ware.TryGetMacro<IEquipment>(x.Macro), x.Count))
                .Where(x => x.Equipment is not null)
                .Select(x => (Equipment: x.Equipment!, x.Count));

            var modulesGridItem = new ModulesGridItem(module);
            foreach (var (equipment, count) in equipments)
            {
                modulesGridItem.Equipments.Add(equipment, count);
            }

            modules.Add(modulesGridItem);
        }



        // 同一モジュールをマージ
        using var dict = new PooledDictionary<int, (int, IX4Module, IWareProduction, long)>();

        foreach (var (module, idx) in modules.Select((x, idx) => (x, idx)))
        {
            var hash = HashCode.Combine(module.Module, module.SelectedMethod);
            if (dict.ContainsKey(hash))
            {
                var tmp = dict[hash];
                tmp.Item4 += module.ModuleCount;
                dict[hash] = tmp;
            }
            else
            {
                dict.Add(hash, (idx, module.Module, module.SelectedMethod, module.ModuleCount));
            }
        }

        // モジュール一覧に追加
        var range = dict.Select(x => (x.Value)).OrderBy(x => x.Item1).Select(x => new ModulesGridItem(x.Item2, x.Item3, x.Item4));
        WorkArea.StationData.ModulesInfo.Modules.AddRange(range);
    }




    /// <summary>
    /// 製品価格を設定
    /// </summary>
    /// <param name="WorkArea"></param>
    /// <param name="saveData"></param>
    private static void SetWarePrice(IWorkArea WorkArea, SaveDataStationItem saveData)
    {
        foreach (var ware in saveData.XElement.XPathSelectElements("/economylog/*[not(self::cargo)]"))
        {
            var wareID = ware.Attribute("ware")?.Value ?? "";
            if (string.IsNullOrEmpty(wareID))
            {
                continue;
            }

            var prod = WorkArea.StationData.ProductsInfo.Products.FirstOrDefault(x => x.Ware.ID == wareID);
            if (prod is not null)
            {
                var priceText = ware.Attribute("price")?.Value;
                prod.UnitPrice = (string.IsNullOrEmpty(priceText)) ? X4Database.Instance.Ware.Get(wareID).AvgPrice : long.Parse(priceText);
            }
        }
    }


    /// <summary>
    /// 保管庫割当状態を設定
    /// </summary>
    /// <param name="WorkArea"></param>
    /// <param name="saveData"></param>
    private static void SetStorageAssign(IWorkArea WorkArea, SaveDataStationItem saveData)
    {
        foreach (var ware in saveData.XElement.XPathSelectElements("overrides/max/ware"))
        {
            var wareID = ware.Attribute("ware")?.Value ?? "";
            if (string.IsNullOrEmpty(wareID))
            {
                continue;
            }

            var storage = WorkArea.StationData.StorageAssignInfo.StorageAssign.FirstOrDefault(x => x.WareID == wareID);
            if (storage is not null)
            {
                var amountText = ware.Attribute("amount")?.Value;
                if (!string.IsNullOrEmpty(amountText))
                {
                    storage.AllocCount = long.Parse(amountText);
                }
            }
        }
    }
}

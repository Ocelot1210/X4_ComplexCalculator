using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Xml.XPath;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.Menu.File.Import.SaveDataImport;

/// <summary>
/// X4のセーブデータからインポートする機能用クラス
/// </summary>
class SaveDataImport : IImport
{
    #region メンバ
    /// <summary>
    /// インポート対象ステーション一覧
    /// </summary>
    private readonly List<SaveDataStationItem> _stations = new();


    /// <summary>
    /// インポート対象ステーション要素番号
    /// </summary>
    private int _stationIdx = 0;
    #endregion


    /// <summary>
    /// メニュー表示用タイトル
    /// </summary>
    public string Title => "X4 セーブデータ";


    /// <summary>
    /// Viewより呼ばれるCommand
    /// </summary>
    public ICommand Command { get; }


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="command">Viewより呼ばれるCommand</param>
    public SaveDataImport(ICommand command)
    {
        Command = command;
    }


    /// <summary>
    /// インポート対象を選択
    /// </summary>
    /// <returns>インポート対象数</returns>
    public int Select()
    {
        var onOK = SelectStationDialog.ShowDialog(_stations);
        if (!onOK)
        {
            _stations.Clear();
        }

        _stationIdx = 0;
        return _stations.Count;
    }


    /// <summary>
    /// インポート実行
    /// </summary>
    /// <param name="WorkArea">作業エリア</param>
    /// <returns>インポートに成功したか</returns>
    public bool Import(IWorkArea WorkArea)
    {
        bool ret;
        try
        {
            ret = ImportMain(WorkArea, _stations[_stationIdx]);
            _stationIdx++;
        }
        catch
        {
            ret = false;
        }

        return ret;
    }


    /// <summary>
    /// インポート実行メイン
    /// </summary>
    /// <param name="WorkArea"></param>
    /// <param name="saveData"></param>
    /// <returns></returns>
    private static bool ImportMain(IWorkArea WorkArea, SaveDataStationItem saveData)
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
        var dict = new Dictionary<int, (int, IX4Module, IWareProduction, long)>();

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

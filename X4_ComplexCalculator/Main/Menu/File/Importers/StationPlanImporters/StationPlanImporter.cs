using Collections.Pooled;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.XPath;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using ZLinq;

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
    /// 同一モジュールマージ用の一時データ
    /// </summary>
    private readonly struct TempModuleData : IDisposable
    {
        /// <summary>
        /// ハッシュ値
        /// </summary>
        private readonly int _hashCode;

        /// <summary>
        /// モジュール
        /// </summary>
        public IX4Module Module { get; }

        /// <summary>
        /// モジュールの装備
        /// </summary>
        public PooledList<(IEquipment Equipment, int Count)> Equipments { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">モジュール</param>
        /// <param name="equipments">モジュールの装備</param>
        /// <param name="order">追加順</param>
        public TempModuleData(IX4Module module, ReadOnlySpan<(IEquipment, int)> equipments)
        {
            Module = module;
            Equipments = equipments.ToPooledList();
            _hashCode = CalcHashCode();
        }


        /// <summary>
        /// ハッシュ値を計算する
        /// </summary>
        /// <returns>このインスタンスのハッシュ値</returns>
        private int CalcHashCode()
        {
            unchecked
            {
                var ret = Module.GetHashCode();
                foreach (var equipment in Equipments)
                {
                    ret = HashCode.Combine(ret, equipment.GetHashCode());
                }

                return ret;
            }
        }


        /// <inheritdoc/>
        public override int GetHashCode() => _hashCode;

        public void Dispose()
        {
            Equipments.Dispose();
        }
    }



    /// <summary>
    /// <see cref="TempModuleData"/> の比較用
    /// </summary>
    private sealed class TempModuleComparer : IEqualityComparer<TempModuleData>
    {
        /// <inheritdoc/>
        public bool Equals(TempModuleData x, TempModuleData y) => x.Module == y.Module && x.Equipments.SequenceEqual(y.Equipments);

        /// <inheritdoc/>
        public int GetHashCode([DisallowNull] TempModuleData obj) => obj.GetHashCode();
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
        using var mergedModules = new PooledDictionary<TempModuleData, int>((int)(double)planItem.Plan.XPathEvaluate("count(entry)"), new TempModuleComparer());

        // ModulesGridItem.Count の変更を行わないようにするため、同一モジュールをマージしつつ xml からデータを読み込む。
        // ※ workArea.StationData.ModulesInfo.Modules 追加前に ModulesGridItem.Count が変更されると困るため
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
                .AsValueEnumerable()
                .Where(x => !string.IsNullOrEmpty(x.Macro))
                .Select(x => (Equipment: X4Database.Instance.Ware.TryGetMacro<IEquipment>(x.Macro), x.Count))
                .Where(x => x.Equipment is not null)
                .GroupBy(x => x.Equipment)
                .Select(x => (Equipment:x.Key!, Count:x.Sum(y => y.Count)))
                .ToArrayPool();

            try
            {
                var item = new TempModuleData(module, new ReadOnlySpan<(IEquipment, int)>(equipments.Array, 0, equipments.Size));

                if (mergedModules.TryGetValue(item, out var count))
                {
                    mergedModules[item] = count + 1;
                    item.Dispose();
                }
                else
                {
                    mergedModules.Add(item, 1);
                }
            }
            finally
            {
                ArrayPool<(IEquipment, int)>.Shared.Return(equipments.Array);
            }
        }


        // マージした一時モジュールデータから、モジュール一覧用のデータを作成・追加
        using var modules = new PooledList<ModulesGridItem>(mergedModules.Count);
        foreach (var (item, moduleCount) in mergedModules)
        {
            var module = new ModulesGridItem(messenger, item.Module, null, moduleCount);
            foreach (var (equipment, equipmentCount) in item.Equipments)
            {
                module.AddEquipment(equipment, equipmentCount);
            }
            modules.Add(module);
        }
        workArea.StationData.ModulesInfo.Modules.AddRange(modules.OrderBy(x => x.Module.Name));

        // ゴミ掃除
        foreach (var key in mergedModules.Keys)
        {
            key.Dispose();
        }
        mergedModules.Clear();


        // 編集状態を全て未編集にする
        IEnumerable<IEditable>[] editables =
        [
            workArea.StationData.ModulesInfo.Modules,
            workArea.StationData.ProductsInfo.Products,
            workArea.StationData.BuildResourcesInfo.BuildResources,
            workArea.StationData.StorageAssignInfo.StorageAssign,
        ];

        foreach (var editable in editables.AsValueEnumerable().SelectMany(x => x))
        {
            editable.EditStatus = EditStatus.Unedited;
        }

        workArea.Title = planItem.PlanName;
        return true;
    }
}

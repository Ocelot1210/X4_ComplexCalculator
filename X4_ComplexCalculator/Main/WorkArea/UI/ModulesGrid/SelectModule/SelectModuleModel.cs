using Collections.Pooled;
using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.SelectModule;

class SelectModuleModel : IDisposable
{
    #region メンバ
    /// <summary>
    /// モジュール追加先
    /// </summary>
    private readonly ObservableRangeCollection<ModulesGridItem> _itemCollection;
    #endregion


    #region プロパティ
    /// <summary>
    /// モジュール種別
    /// </summary>
    public ObservablePropertyChangedCollection<ModulesListItem> ModuleTypes { get; } = new();


    /// <summary>
    /// モジュール所有派閥
    /// </summary>
    public ObservablePropertyChangedCollection<FactionsListItem> ModuleOwners { get; } = new();


    /// <summary>
    /// モジュール一覧
    /// </summary>
    public ObservablePropertyChangedCollection<ModulesListItem> Modules { get; } = new();
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="itemCollection">選択結果格納先</param>
    public SelectModuleModel(ObservableRangeCollection<ModulesGridItem> itemCollection)
    {
        _itemCollection = itemCollection;

        ModuleOwners.CollectionPropertyChanged += UpdateModules;
        ModuleTypes.CollectionPropertyChanged += UpdateModules;

        InitModuleTypes();
        InitModuleOwners();
        UpdateModulesMain();
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        ModuleTypes.CollectionPropertyChanged -= UpdateModules;
        ModuleOwners.CollectionPropertyChanged -= UpdateModules;
    }


    /// <summary>
    /// モジュール種別一覧を初期化する
    /// </summary>
    private void InitModuleTypes()
    {
        const string SQL_1 = @"SELECT ModuleTypeID, Name FROM ModuleType WHERE ModuleTypeID IN (SELECT ModuleTypeID FROM Module) ORDER BY Name";

        using var items = new PooledList<ModulesListItem>();
        foreach (var (moduleTypeID, name) in X4Database.Instance.Query<(string, string)>(SQL_1))
        {
            const string SQL_2 = @"SELECT count(*) AS Count FROM SelectModuleCheckStateModuleTypes WHERE ID = :ID";
            var @checked = 0 < SettingDatabase.Instance.QuerySingle<long>(SQL_2, new { ID = moduleTypeID });
            items.Add(new ModulesListItem(moduleTypeID, name, @checked));
        }


        ModuleTypes.AddRange(items);
    }


    /// <summary>
    /// 派閥一覧を初期化する
    /// </summary>
    private void InitModuleOwners()
    {
        const string SQL_1 = @"SELECT FactionID, Name FROM Faction WHERE FactionID IN (SELECT FactionID FROM WareOwner) ORDER BY Name";

        using var items = new PooledList<FactionsListItem>();

        var factions = X4Database.Instance.Query<string>(SQL_1)
            .Select(x => X4Database.Instance.Faction.TryGet(x))
            .Where(x => x is not null)
            .Select(x => x!);

        foreach (var faction in factions)
        {
            const string SQL_2 = @"SELECT count(*) AS Count FROM SelectModuleCheckStateModuleOwners WHERE ID = :ID";
            var @checked = 0 < SettingDatabase.Instance.QuerySingle<long>(SQL_2, new { ID = faction.FactionID });
            items.Add(new FactionsListItem(faction, @checked));
        }

        ModuleOwners.AddRange(items);
    }


    /// <summary>
    /// モジュール一覧を更新する
    /// </summary>
    private void UpdateModules(object sender, EventArgs e)
    {
        UpdateModulesMain();
    }


    /// <summary>
    /// モジュール一覧を更新する
    /// </summary>
    private void UpdateModulesMain()
    {
        var checkedModuleTypes = new HashSet<string>(ModuleTypes.Where(x => x.IsChecked).Select(x => x.ID));

        var checkedOwners = ModuleOwners
            .Where(x => x.IsChecked)
            .Select(x => x.Faction.FactionID)
            .ToArray();

        var newModules = X4Database.Instance.Ware.GetAll<IX4Module>()
            .Where(x => 
                !(x.Tags.Contains("noplayerblueprint") || x.Tags.Contains("noblueprint")) &&
                checkedModuleTypes.Contains(x.ModuleType.ModuleTypeID) &&
                checkedOwners.Intersect(x.Owners.Select(y => y.FactionID)).Any())
            .Select(x => new ModulesListItem(x));

        Modules.Reset(newModules);
    }


    /// <summary>
    /// 選択中のモジュール一覧をコレクションに追加する
    /// </summary>
    public void AddSelectedModuleToItemCollection()
    {
        // 選択されているアイテムを追加
        var items = Modules.Where(x => x.IsChecked)
            .Select(x => X4Database.Instance.Ware.TryGet<IX4Module>(x.ID))
            .Where(x => x is not null)
            .Select(x => new ModulesGridItem(x!) { EditStatus = EditStatus.Edited });

        _itemCollection.AddRange(items);
    }

    /// <summary>
    /// チェック状態を保存する
    /// </summary>
    public void SaveCheckState() => SettingDatabase.Instance.BeginTransaction(db =>
    {
        // 前回値クリア
        db.Execute("DELETE FROM SelectModuleCheckStateModuleTypes");
        db.Execute("DELETE FROM SelectModuleCheckStateModuleOwners");

        // モジュール種別のチェック状態保存
        var checkedTypes = ModuleTypes.Where(x => x.IsChecked);
        db.Execute("INSERT INTO SelectModuleCheckStateModuleTypes(ID) VALUES (:ID)", checkedTypes);

        // 派閥一覧のチェック状態保存
        var checkedFactions = ModuleOwners.Where(x => x.IsChecked).Select(x => x.Faction);
        db.Execute("INSERT INTO SelectModuleCheckStateModuleOwners(ID) VALUES (:FactionID)", checkedFactions);
    });
}

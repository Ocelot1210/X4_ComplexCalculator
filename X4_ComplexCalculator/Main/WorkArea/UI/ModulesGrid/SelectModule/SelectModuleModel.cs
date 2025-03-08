using Collections.Pooled;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Linq;
using X4_ComplexCalculator.Common.Collections;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.SelectModule.Entities;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.SelectModule;

sealed class SelectModuleModel : ObservableRecipient
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
    public ObservableRangeCollection<ModuleTypeListItem> ModuleTypes { get; } = [];


    /// <summary>
    /// モジュール所有派閥
    /// </summary>
    public ObservableRangeCollection<ModuleOwnersListItem> ModuleOwners { get; } = [];


    /// <summary>
    /// モジュール一覧
    /// </summary>
    public ObservableRangeCollection<ModulesListItem> Modules { get; } = [];
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messanger">メッセージ交換用</param>
    /// <param name="itemCollection">選択結果格納先</param>
    public SelectModuleModel(IMessenger messenger, ObservableRangeCollection<ModulesGridItem> itemCollection) : base(messenger)
    {
        _itemCollection = itemCollection;

        InitModuleTypes();
        InitModuleOwners();
        InitModules();
    }


    /// <summary>
    /// モジュール種別一覧を初期化する
    /// </summary>
    private void InitModuleTypes()
    {
        const string SQL_1 = @"SELECT ModuleTypeID, Name FROM ModuleType WHERE ModuleTypeID IN (SELECT ModuleTypeID FROM Module) ORDER BY Name";

        using var items = new PooledList<ModuleTypeListItem>();
        foreach (var (moduleTypeID, name) in X4Database.Instance.Query<(string, string)>(SQL_1))
        {
            const string SQL_2 = @"SELECT count(*) AS Count FROM SelectModuleCheckStateModuleTypes WHERE ID = :ID";
            var @checked = 0 < SettingDatabase.Instance.QuerySingle<long>(SQL_2, new { ID = moduleTypeID });
            items.Add(new ModuleTypeListItem(Messenger, moduleTypeID, name, @checked));
        }


        ModuleTypes.AddRange(items);
    }


    /// <summary>
    /// 派閥一覧を初期化する
    /// </summary>
    private void InitModuleOwners()
    {
        const string SQL_1 = @"SELECT FactionID, Name FROM Faction WHERE FactionID IN (SELECT FactionID FROM WareOwner) ORDER BY Name";

        using var items = new PooledList<ModuleOwnersListItem>();

        var factions = X4Database.Instance.Query<string>(SQL_1)
            .Select(X4Database.Instance.Faction.TryGet)
            .Where(x => x is not null)
            .Select(x => x!);

        foreach (var faction in factions)
        {
            const string SQL_2 = @"SELECT count(*) AS Count FROM SelectModuleCheckStateModuleOwners WHERE ID = :ID";
            var @checked = 0 < SettingDatabase.Instance.QuerySingle<long>(SQL_2, new { ID = faction.FactionID });
            items.Add(new ModuleOwnersListItem(Messenger, faction, @checked));
        }

        ModuleOwners.AddRange(items);
    }


    /// <summary>
    /// モジュール一覧を初期化する
    /// </summary>
    private void InitModules()
    {
        var modules = X4Database.Instance.Ware.GetAll<IX4Module>()
            .Where(x => !(x.Tags.Contains("noplayerblueprint") || x.Tags.Contains("noblueprint")))
            .Select(x => new ModulesListItem(x.ID, x.Name, false));

        Modules.AddRange(modules);
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
            .Select(x => new ModulesGridItem(Messenger, x!) { EditStatus = EditStatus.Edited });

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
        var checkedFactions = ModuleOwners.Where(x => x.IsChecked);
        db.Execute("INSERT INTO SelectModuleCheckStateModuleOwners(ID) VALUES (:FactionID)", checkedFactions);
    });
}

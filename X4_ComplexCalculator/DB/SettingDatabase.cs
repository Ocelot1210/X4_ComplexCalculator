﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB;

/// <summary>
/// 設定データベースの読み込み・書き込みを行うクラス
/// </summary>
class SettingDatabase : DBConnection
{
    #region スタティックメンバ
    /// <summary>
    /// インスタンス
    /// </summary>
    private static SettingDatabase? _Instance;
    #endregion


    #region プロパティ(static)
    /// <summary>
    /// インスタンス
    /// </summary>
    public static SettingDatabase Instance
        => _Instance ?? throw new InvalidOperationException();
    #endregion


    /// <summary>
    /// 設定データベースを開く
    /// </summary>
    /// <param name="dbPath">データベースの絶対パス</param>
    private SettingDatabase(string dbPath) : base(dbPath) { }


    /// <summary>
    /// 設定データベースを開く
    /// </summary>
    /// <returns>設定データベース</returns>
    public static void Open()
    {
        if (_Instance is not null) return;

        var basePath = AppDomain.CurrentDomain.BaseDirectory ?? "";
        var dbPath = Path.Combine(basePath, Configuration.Instance.CommonDBPath);

        _Instance = new SettingDatabase(dbPath);
        _Instance.BeginTransaction(db =>
        {
            db.Execute("CREATE TABLE IF NOT EXISTS SelectModuleCheckStateModuleTypes(ID TEXT NOT NULL)");
            db.Execute("CREATE TABLE IF NOT EXISTS SelectModuleCheckStateModuleOwners(ID TEXT NOT NULL)");
            db.Execute("CREATE TABLE IF NOT EXISTS SelectModuleEquipmentCheckStateFactions(ID TEXT NOT NULL)");
            db.Execute("CREATE TABLE IF NOT EXISTS ModulePresets(ModuleID TEXT NOT NULL, PresetID INTEGER NOT NULL, PresetName TEXT NOT NULL)");
            db.Execute("CREATE TABLE IF NOT EXISTS ModulePresetsEquipment(ModuleID TEXT NOT NULL, PresetID INTEGER NOT NULL, EquipmentID TEXT NOT NULL, EquipmentType TEXT NOT NULL)");
            db.Execute("CREATE TABLE IF NOT EXISTS WorkAreaLayouts(LayoutID INTEGER NOT NULL, LayoutName TEXT NOT NULL, IsChecked BOOLEAN DEFAULT 0, Layout BLOB NOT NULL)");
            db.Execute("CREATE TABLE IF NOT EXISTS OpenedFiles(Path TEXT NOT NULL)");
        });

        // WorkAreaLayouts の IsChecked の型が INTEGER の場合、 BOOLEAN に修正する
        if (_Instance.QuerySingle<bool>("SELECT TYPE = 'INTEGER' FROM PRAGMA_TABLE_INFO('WorkAreaLayouts') WHERE NAME = 'IsChecked'"))
        {
            _Instance.BeginTransaction(db =>
            {
                db.Execute("CREATE TABLE TEMP(LayoutID INTEGER NOT NULL, LayoutName TEXT NOT NULL, IsChecked BOOLEAN DEFAULT 0, Layout BLOB NOT NULL)");
                db.Execute("INSERT INTO TEMP SELECT * FROM WorkAreaLayouts");
                db.Execute("DROP TABLE WorkAreaLayouts");
                db.Execute("ALTER TABLE TEMP RENAME TO WorkAreaLayouts");
            });
        }
    }


    /// <summary>
    /// 指定したモジュールに対する使用可能なプリセットIDを返す
    /// </summary>
    /// <param name="moduleID">指定したモジュールID</param>
    /// <returns>使用可能なプリセットID</returns>
    public long GetLastModulePresetsID(string moduleID)
    {
        const string SQL = @"
SELECT
    ifnull(MIN( PresetID + 1 ), 0) AS PresetID
FROM
    ModulePresets
WHERE
	ModuleID = :ModuleID AND
    ( PresetID + 1 ) NOT IN ( SELECT PresetID FROM ModulePresets WHERE ModuleID = :ModuleID)";

        return QuerySingle<long>(SQL, new { ModuleID = moduleID });
    }


    /// <summary>
    /// プリセット名を更新する
    /// </summary>
    /// <param name="moduleID"></param>
    /// <param name="presetID"></param>
    /// <param name="newPresetName"></param>
    public void UpdateModulePresetName(string moduleID, long presetID, string newPresetName)
    {
        var param = new
        {
            ModuleID = moduleID,
            PresetID = presetID,
            PresetName = newPresetName,
        };

        Execute("UPDATE ModulePresets Set PresetName = :PresetName WHERE ModuleID = :ModuleID AND presetID = :PresetID", param);
    }


    /// <summary>
    /// 装備編集画面でチェックされた派閥一覧を取得する
    /// </summary>
    /// <returns>装備編集画面でチェックされた派閥一覧のHashSet</returns>
    public HashSet<string> GetCheckedFactionsAtSelectEquipmentWindow()
    {
        return new HashSet<string>(Query<string>("SELECT ID FROM SelectModuleEquipmentCheckStateFactions"));
    }


    /// <summary>
    /// 装備編集画面でチェックされた派閥一覧を設定する
    /// </summary>
    /// <param name="checkedFactions">装備編集画面でチェックされた派閥一覧</param>
    public void SetCheckedFactionsAtSelectEquipmentWindow(IEnumerable<IFaction> checkedFactions)
    {
        BeginTransaction(db =>
        {
            // 前回値クリア
            db.Execute("DELETE FROM SelectModuleEquipmentCheckStateFactions");

            // チェック済みの派閥追加
            db.Execute("INSERT INTO SelectModuleEquipmentCheckStateFactions(ID) VALUES (:FactionID)", checkedFactions);
        });
    }


    /// <summary>
    /// 指定したモジュールのプリセットを取得する
    /// </summary>
    /// <param name="moduleID">指定したモジュールのID</param>
    /// <returns>指定したモジュールに対応するプリセットの列挙</returns>
    public IEnumerable<(long ID, string Name)> GetModulePreset(string moduleID)
    {
        return Query<(long, string)>("SELECT DISTINCT PresetID, PresetName FROM ModulePresets WHERE ModuleID = @ModuleID", new { ModuleID = moduleID });
    }



    /// <summary>
    /// モジュールのプリセットを追加
    /// </summary>
    /// <param name="moduleID">モジュールID</param>
    /// <param name="presetID">プリセットID</param>
    /// <param name="presetName">プリセット名</param>
    /// <param name="equipments">装備一覧</param>
    public void AddModulePreset(string moduleID, long presetID, string presetName, IEnumerable<IEquipment> equipments)
    {
        BeginTransaction(db =>
        {
            db.Execute(
                "INSERT INTO ModulePresets(ModuleID, PresetID, PresetName) VALUES(:ModuleID, :PresetID, :PresetName)",
                new { ModuleID = moduleID, PresetID = presetID, PresetName = presetName }
            );


            var data = equipments
                .Select(x => new { ModuleID = moduleID, PresetID = presetID, EquipmentID = x.ID, x.EquipmentType.EquipmentTypeID });

            db.Execute(
                "INSERT INTO ModulePresetsEquipment(ModuleID, PresetID, EquipmentID, EquipmentType) VALUES(@ModuleID, @PresetID, @EquipmentID, @EquipmentTypeID)",
                data
            );
        });
    }

    
    /// <summary>
    /// モジュールのプリセットを削除する
    /// </summary>
    /// <param name="moduleID">モジュールID</param>
    /// <param name="presetID">プリセットID</param>
    public void DeleteModulePreset(string moduleID, long presetID)
    {
        BeginTransaction(db =>
        {
            var param = new { ModuleID = moduleID, PresetID = presetID };

            db.Execute(
                "DELETE FROM ModulePresets WHERE ModuleID = :ModuleID AND PresetID = :PresetID",
                param
            );

            db.Execute(
                "DELETE FROM ModulePresetsEquipment WHERE ModuleID = :ModuleID AND PresetID = :PresetID",
                param
            );
        });
    }


    /// <summary>
    /// プリセットを上書き保存する
    /// </summary>
    /// <param name="moduleID">モジュールID</param>
    /// <param name="presetID">プリセットID</param>
    /// <param name="equipments">装備一覧</param>
    public void OverwritePreset(string moduleID, long presetID, IEnumerable<IEquipment> equipments)
    {
        BeginTransaction(db =>
        {
            // 前回値削除
            db.Execute(
                "DELETE FROM ModulePresetsEquipment WHERE ModuleID = :ModuleID AND PresetID = :PresetID",
                new { ModuleID = moduleID, PresetID = presetID }
            );


            var data = equipments
                .Select(x => new { ModuleID = moduleID, PresetID = presetID, EquipmentID = x.ID, x.EquipmentType.EquipmentTypeID });

            db.Execute(
                "INSERT INTO ModulePresetsEquipment(ModuleID, PresetID, EquipmentID, EquipmentType) VALUES(@ModuleID, @PresetID, @EquipmentID, @EquipmentTypeID)",
                data
            );
        });
    }


    /// <summary>
    /// 空いている(使用可能な)レイアウトIDを取得する
    /// </summary>
    /// <returns>空いている(使用可能な)レイアウトID</returns>
    public long GetLastLayoutID()
    {
        // 空いているレイアウトIDを取得する
        var sql = @"
SELECT
    ifnull(MIN( LayoutID + 1 ), 0) AS LayoutID
FROM
    WorkAreaLayouts
WHERE
    ( LayoutID + 1 ) NOT IN ( SELECT LayoutID FROM WorkAreaLayouts)";

        return QuerySingle<long>(sql);
    }
}

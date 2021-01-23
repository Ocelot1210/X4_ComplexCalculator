using System;
using System.IO;
using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.DB
{
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

            var config = Configuration.GetConfiguration();
            var basePath = AppDomain.CurrentDomain.BaseDirectory ?? "";
            var dbPath = Path.Combine(basePath, config["AppSettings:CommonDBPath"]);

            _Instance = new SettingDatabase(dbPath);
            _Instance.ExecQuery("CREATE TABLE IF NOT EXISTS SelectModuleCheckStateModuleTypes(ID TEXT NOT NULL)");
            _Instance.ExecQuery("CREATE TABLE IF NOT EXISTS SelectModuleCheckStateModuleOwners(ID TEXT NOT NULL)");
            _Instance.ExecQuery("CREATE TABLE IF NOT EXISTS SelectModuleEquipmentCheckStateFactions(ID TEXT NOT NULL)");
            _Instance.ExecQuery("CREATE TABLE IF NOT EXISTS ModulePresets(ModuleID TEXT NOT NULL, PresetID INTEGER NOT NULL, PresetName TEXT NOT NULL)");
            _Instance.ExecQuery("CREATE TABLE IF NOT EXISTS ModulePresetsEquipment(ModuleID TEXT NOT NULL, PresetID INTEGER NOT NULL, EquipmentID TEXT NOT NULL, EquipmentType TEXT NOT NULL)");
            _Instance.ExecQuery("CREATE TABLE IF NOT EXISTS WorkAreaLayouts(LayoutID INTEGER NOT NULL, LayoutName TEXT NOT NULL, IsChecked BOOLEAN DEFAULT 0, Layout BLOB NOT NULL)");
            _Instance.ExecQuery("CREATE TABLE IF NOT EXISTS OpenedFiles(Path TEXT NOT NULL)");

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
            const string sql = @"
SELECT
    ifnull(MIN( PresetID + 1 ), 0) AS PresetID
FROM
    ModulePresets
WHERE
	ModuleID = :ModuleID AND
    ( PresetID + 1 ) NOT IN ( SELECT PresetID FROM ModulePresets WHERE ModuleID = :ModuleID)";

            return QuerySingle<long>(sql, new { ModuleID = moduleID });
        }


        /// <summary>
        /// プリセットを追加する
        /// </summary>
        /// <param name="moduleID"></param>
        /// <param name="presetName"></param>
        /// <returns>追加されたプリセットID</returns>
        public long AddModulePreset(string moduleID, string presetName)
        {
            var presetID = GetLastModulePresetsID(moduleID);

            var param = new SQLiteCommandParameters(3);
            param.Add("moduleID",   System.Data.DbType.String, moduleID);
            param.Add("presetID",   System.Data.DbType.Int64, presetID);
            param.Add("presetName", System.Data.DbType.String, presetName);

            ExecQuery($"INSERT INTO ModulePresets(ModuleID, PresetID, PresetName) VALUES(:moduleID, :presetID, :presetName)", param);

            return presetID;
        }


        /// <summary>
        /// プリセット名を更新する
        /// </summary>
        /// <param name="moduleID"></param>
        /// <param name="presetID"></param>
        /// <param name="newPresetName"></param>
        public void UpdateModulePresetName(string moduleID, long presetID, string newPresetName)
        {
            var param = new SQLiteCommandParameters(3);
            param.Add("moduleID",   System.Data.DbType.String, moduleID);
            param.Add("presetID",   System.Data.DbType.Int64, presetID);
            param.Add("presetName", System.Data.DbType.String, newPresetName);

            ExecQuery($"UPDATE ModulePresets Set PresetName = :presetName WHERE ModuleID = :moduleID AND presetID = :presetID", param);
        }
    }
}

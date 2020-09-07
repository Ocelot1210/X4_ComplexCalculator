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
        public static SettingDatabase? _Instance;
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
            if (_Instance != null) return;

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
    }
}

using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.FileSystem;
using LibX4.Xml;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// モジュールの保管容量情報抽出用クラス
    /// </summary>
    class ModuleStorageExporter : IExporter
    {
        /// <summary>
        /// catファイルオブジェクト
        /// </summary>
        private readonly IIndexResolver _CatFile;

        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;

        /// <summary>
        /// 保管庫種別一覧
        /// </summary>
        private readonly LinkedList<ModuleStorageType> _StorageTypes = new();


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="waresXml">ウェア情報xml</param>
        public ModuleStorageExporter(IIndexResolver catFile, XDocument waresXml)
        {
            _CatFile = catFile;
            _WaresXml = waresXml;
        }


        /// <summary>
        /// 抽出処理
        /// </summary>
        /// <param name="connection"></param>
        public void Export(IDbConnection connection)
        {
            //////////////////
            // テーブル作成 //
            //////////////////
            {
                connection.Execute(@"
CREATE TABLE IF NOT EXISTS ModuleStorage
(
    ModuleID        TEXT    NOT NULL PRIMARY KEY,
    Amount          INTEGER NOT NULL,
    FOREIGN KEY (ModuleID)  REFERENCES Module(ModuleID)
) WITHOUT ROWID");

                connection.Execute(@"
CREATE TABLE IF NOT EXISTS ModuleStorageType
(
    ModuleID        TEXT    NOT NULL,
    TransportTypeID TEXT    NOT NULL,
    PRIMARY KEY (ModuleID, TransportTypeID),
    FOREIGN KEY (ModuleID)          REFERENCES Module(ModuleID),
    FOREIGN KEY (TransportTypeID)   REFERENCES TransportType(TransportTypeID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute("INSERT INTO ModuleStorage (ModuleID, Amount) VALUES (@ModuleID, @Amount)", items);

                connection.Execute("INSERT INTO ModuleStorageType (ModuleID, TransportTypeID) VALUES (@ModuleID, @TransportTypeID)", _StorageTypes);
            }
        }


        /// <summary>
        /// XML から ModuleStorage データを読み出す
        /// </summary>
        /// <returns>読み出した ModuleStorage データ</returns>
        private IEnumerable<ModuleStorage> GetRecords()
        {
            foreach (var module in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'module')]"))
            {
                var moduleID = module.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(moduleID)) continue;

                var macroName = module.XPathSelectElement("component").Attribute("ref").Value;
                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);

                // 容量が記載されている箇所を抽出
                var cargo = macroXml.Root.XPathSelectElement("macro/properties/cargo");
                if (cargo == null) continue;

                // 保管庫種別を取得する
                var transportTypeExists = false;
                foreach (var transportTypeID in Util.SplitTags(cargo.Attribute("tags")?.Value))
                {
                    transportTypeExists = true;
                    _StorageTypes.AddLast(new ModuleStorageType(moduleID, transportTypeID));
                }

                // 保管庫種別が存在しなければ無効なデータと見なして登録しない
                if (!transportTypeExists) continue;

                var amount = cargo.Attribute("max").GetInt();

                yield return new ModuleStorage(moduleID, amount);
            }
        }
    }
}

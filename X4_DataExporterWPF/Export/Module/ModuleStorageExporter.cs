using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.FileSystem;
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
        private readonly CatFile _CatFile;

        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="waresXml">ウェア情報xml</param>
        public ModuleStorageExporter(CatFile catFile, XDocument waresXml)
        {
            _CatFile = catFile;
            _WaresXml = waresXml;
        }


        /// <summary>
        /// 抽出処理
        /// </summary>
        /// <param name="cmd"></param>
        public void Export(IDbConnection connection)
        {
            //////////////////
            // テーブル作成 //
            //////////////////
            {
                connection.Execute(@"
CREATE TABLE IF NOT EXISTS ModuleStorage
(
    ModuleID        TEXT    NOT NULL,
    TransportTypeID TEXT    NOT NULL,
    Amount          INTEGER NOT NULL,
    PRIMARY KEY (ModuleID, TransportTypeID),
    FOREIGN KEY (ModuleID)          REFERENCES Module(ModuleID),
    FOREIGN KEY (TransportTypeID)   REFERENCES TransportType(TransportTypeID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = _WaresXml.Root.XPathSelectElements("ware[@tags='module']").Select
                (
                    module => GetRecord(module)
                )
                .Where
                (
                    x => x != null
                );

                connection.Execute("INSERT INTO ModuleStorage (ModuleID, TransportTypeID, Amount) VALUES (@ModuleID, @TransportTypeID, @Amount)", items);
            }
        }


        /// <summary>
        /// 1レコード分の情報抽出
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        private ModuleStorage? GetRecord(XElement module)
        {
            try
            {
                var moduleID = module.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(moduleID)) return null;

                var macroName = module.XPathSelectElement("component").Attribute("ref").Value;
                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);

                // 容量が記載されている箇所を抽出
                var cargo = macroXml.Root.XPathSelectElement("macro/properties/cargo");

                // 総合保管庫は飛ばす
                var transportTypeID = cargo?.Attribute("tags")?.Value;
                if (string.IsNullOrEmpty(transportTypeID)) return null;
                if (transportTypeID.Contains(' ') == true) return null;

                var amount = int.Parse(cargo?.Attribute("max")?.Value ?? "");

                return new ModuleStorage(moduleID, transportTypeID, amount);
            }
            catch
            {
                return null;
            }
        }
    }
}

using Dapper;
using LibX4.FileSystem;
using LibX4.Xml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;
using X4_DataExporterWPF.Internal;

namespace X4_DataExporterWPF.Export;

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
        ArgumentNullException.ThrowIfNull(waresXml.Root);

        _CatFile = catFile;
        _WaresXml = waresXml;
    }


    /// <inheritdoc/>
    public async Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            await connection.ExecuteAsync(@"
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
            var items = GetRecordsAsync(progress, cancellationToken);

            await connection.ExecuteAsync("INSERT INTO ModuleStorage (ModuleID, Amount) VALUES (@ModuleID, @Amount)", items);

            await connection.ExecuteAsync("INSERT INTO ModuleStorageType (ModuleID, TransportTypeID) VALUES (@ModuleID, @TransportTypeID)", _StorageTypes);
        }
    }


    /// <summary>
    /// XML から ModuleStorage データを読み出す
    /// </summary>
    /// <returns>読み出した ModuleStorage データ</returns>
    private async IAsyncEnumerable<ModuleStorage> GetRecordsAsync(IProgress<(int currentStep, int maxSteps)> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var maxSteps = (int)(double)_WaresXml.Root!.XPathEvaluate("count(ware[contains(@tags, 'module')])");
        var currentStep = 0;


        foreach (var module in _WaresXml.Root!.XPathSelectElements("ware[contains(@tags, 'module')]"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report((currentStep++, maxSteps));


            var moduleID = module.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(moduleID)) continue;

            var macroName = module.XPathSelectElement("component")?.Attribute("ref")?.Value;
            if (string.IsNullOrEmpty(macroName)) continue;

            var macroXml = await _CatFile.OpenIndexXmlAsync("index/macros.xml", macroName, cancellationToken);
            if (macroXml?.Root is null) continue;

            // 容量が記載されている箇所を抽出
            var cargo = macroXml.Root.XPathSelectElement("macro/properties/cargo");
            if (cargo is null) continue;

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

        progress.Report((currentStep++, maxSteps));
    }
}

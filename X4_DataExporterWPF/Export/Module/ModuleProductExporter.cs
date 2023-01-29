using Dapper;
using LibX4.FileSystem;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;
using LibX4.Xml;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;
using X4_DataExporterWPF.Internal;

namespace X4_DataExporterWPF.Export;

/// <summary>
/// モジュールの生産品情報抽出用クラス
/// </summary>
public class ModuleProductExporter : IExporter
{
    /// <summary>
    /// catファイルオブジェクト
    /// </summary>
    private readonly IIndexResolver _catFile;

    /// <summary>
    /// ウェア情報xml
    /// </summary>
    private readonly XDocument _waresXml;

    /// <summary>
    /// デフォルト値が格納されているxml
    /// </summary>
    private readonly XDocument _defaultsXml;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="catFile">catファイルオブジェクト</param>
    /// <param name="waresXml">ウェア情報xml</param>
    public ModuleProductExporter(IIndexResolver catFile, XDocument waresXml, XDocument defaultsXml)
    {
        ArgumentNullException.ThrowIfNull(waresXml.Root);
        ArgumentNullException.ThrowIfNull(defaultsXml.Root);

        _catFile = catFile;
        _waresXml = waresXml;
        _defaultsXml = defaultsXml;
    }


    /// <inheritdoc/>
    public async Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            await connection.ExecuteAsync(@"
CREATE TABLE IF NOT EXISTS ModuleProduct
(
    ModuleID    TEXT    NOT NULL,
    WareID      TEXT    NOT NULL,
    Method      TEXT    NOT NULL,
    Amount      INTEGER,
    PRIMARY KEY (ModuleID, WareID, Method),
    FOREIGN KEY (ModuleID)  REFERENCES Module(ModuleID),
    FOREIGN KEY (WareID)    REFERENCES Ware(WareID)
)WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecordsAsync(progress, cancellationToken);

            await connection.ExecuteAsync("INSERT INTO ModuleProduct (ModuleID, WareID, Method, Amount) VALUES (@ModuleID, @WareID, @Method, @Amount)", items);
        }
    }


    /// <summary>
    /// XML から ModuleProduct データを読み出す
    /// </summary>
    /// <returns>読み出した ModuleProduct データ</returns>
    private async IAsyncEnumerable<ModuleProduct> GetRecordsAsync(IProgress<(int currentStep, int maxSteps)> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var maxSteps = (int)(double)_waresXml.Root!.XPathEvaluate("count(ware[contains(@tags, 'module')])");
        var currentStep = 0;

        foreach (var module in _waresXml.Root!.XPathSelectElements("ware[contains(@tags, 'module')]"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report((currentStep++, maxSteps));

            var moduleID = module.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(moduleID)) continue;

            var macroName = module.XPathSelectElement("component")?.Attribute("ref")?.Value;
            if (string.IsNullOrEmpty(macroName)) continue;

            var macroXml = await _catFile.OpenIndexXmlAsync("index/macros.xml", macroName, cancellationToken);
            if (macroXml?.Root is null) continue;

            //////////////////////////////////////////////////
            // *_macro.xml からモジュールの生産品情報を抽出 //
            //////////////////////////////////////////////////
            foreach (var queue in macroXml.Root.XPathSelectElements("macro/properties/production/queue"))
            {
                var wareID = queue.Attribute("ware")?.Value ?? "";
                var method = queue.Attribute("method")?.Value ?? "default";

                if (!string.IsNullOrEmpty(wareID))
                {
                    yield return new ModuleProduct(moduleID, wareID, method, 1);
                }

                foreach (var item in queue.Elements("item"))
                {
                    wareID = item.Attribute("ware")?.Value ?? "";
                    method = item.Attribute("method")?.Value ?? "default";

                    if (!string.IsNullOrEmpty(wareID))
                    {
                        yield return new ModuleProduct(moduleID, wareID, method, 1);
                    }
                }
            }


            ///////////////////////////////////////////////////
            // defaults.xml からモジュールの生産品情報を抽出 //
            ///////////////////////////////////////////////////
            {
                var className = macroXml.Root.XPathSelectElement("macro")?.Attribute("class")?.Value;
                if (!string.IsNullOrEmpty(className))
                {
                    var dataSet = _defaultsXml.Root!.XPathSelectElement($"dataset[@class='{className}']");
                    if (dataSet is not null)
                    {
                        var product = dataSet.XPathSelectElement("properties/product");
                        var wareID = product?.Attribute("ware")?.Value;
                        var amount = product?.Attribute("amount")?.GetInt();
                        var method = dataSet.XPathSelectElement("properties/purpose")?.Attribute("primary")?.Value;

                        if (!string.IsNullOrEmpty(wareID) && amount.HasValue && !string.IsNullOrEmpty(method))
                        {
                            yield return new ModuleProduct(moduleID, wareID, method, amount.Value);
                        }
                    }
                }
            }
        }

        progress.Report((currentStep++, maxSteps));
    }
}

﻿using Dapper;
using LibX4.FileSystem;
using LibX4.Lang;
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
/// 用途抽出用クラス
/// </summary>
public class PurposeExporter : IExporter
{
    /// <summary>
    /// catファイルオブジェクト
    /// </summary>
    private readonly ICatFile _catFile;


    /// <summary>
    /// ウェア情報xml
    /// </summary>
    private readonly XDocument _waresXml;


    /// <summary>
    /// 言語解決用オブジェクト
    /// </summary>
    private readonly ILanguageResolver _resolver;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="catFile">catファイルオブジェクト</param>
    /// <param name="waresXml">ウェア情報xml</param>
    /// <param name="resolver">言語解決用オブジェクト</param>
    public PurposeExporter(ICatFile catFile, XDocument waresXml, ILanguageResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(waresXml.Root);

        _catFile = catFile;
        _waresXml = waresXml;
        _resolver = resolver;
    }


    /// <inheritdoc/>
    public async Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            await connection.ExecuteAsync(@"
CREATE TABLE IF NOT EXISTS Purpose
(
    PurposeID   TEXT    NOT NULL PRIMARY KEY,
    Name        TEXT    NOT NULL
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecordsAsync(progress, cancellationToken);

            await connection.ExecuteAsync("INSERT INTO Purpose (PurposeID, Name) VALUES (@PurposeID, @Name)", items);
        }
    }


    /// <summary>
    /// XML から Purpose データを読み出す
    /// </summary>
    /// <returns>読み出した Purpose データ</returns>
    private async IAsyncEnumerable<Purpose> GetRecordsAsync(IProgress<(int currentStep, int maxSteps)> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // TODO: 可能ならファイルから抽出する
        // 注) 「IDは仮」と記載されている項目は 0001-l044.xml を参考にした
        var names = new Dictionary<string, string>
        {
            {"universal",   "{20213,  100}"},       // 全般  (IDは仮)
            {"trade",       "{20213,  200}"},       // 交易
            {"fight",       "{20213,  300}"},       // 戦闘
            {"build",       "{20213,  400}"},       // 建築
            {"mine",        "{20213,  500}"},       // 採掘
            {"hack",        "{20213,  600}"},       // ハッキング (IDは仮)
            {"scan",        "{20213,  700}"},       // スキャン   (IDは仮)
            {"production",  "{20213,  800}"},       // 製造
            {"storage",     "{20213,  900}"},       // 保管
            {"connection",  "{20213, 1000}"},       // 接続
            {"habitation",  "{20213, 1100}"},       // 居住
            {"defence",     "{20213, 1200}"},       // 防衛
            {"docking",     "{20213, 1300}"},       // ドッキング
            {"venture",     "{20213, 1400}"},       // 探検
            {"auxiliary",   "{20213, 1500}"},       // 採掘
            {"welfare",     "{20213, 1600}"},       // 福祉 (IDは仮)
            {"processing",  "{20213, 1700}"},       // 処理 (IDは仮)
            {"salvage",     "{20213, 1800}"},       // 引き揚げ
            {"dismantling", "{20213, 1900}"},       // 解体 (IDは仮)
        };


        var maxSteps = (int)(double)_waresXml.Root!.XPathEvaluate("count(ware[contains(@tags, 'ship')])");
        var currentStep = 0;
        var added = new HashSet<string>();

        foreach (var ship in _waresXml.Root!.XPathSelectElements("ware[contains(@tags, 'ship')]"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((currentStep++, maxSteps));

            var shipID = ship.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(shipID)) continue;

            var macroName = ship.XPathSelectElement("component")?.Attribute("ref")?.Value;
            if (string.IsNullOrEmpty(macroName)) continue;

            var macroXml = await _catFile.OpenIndexXmlAsync("index/macros.xml", macroName, cancellationToken);
            if (macroXml.Root is null) continue;

            foreach (var elm in macroXml.Root.XPathSelectElements("macro/properties/purpose"))
            {
                foreach (var attr in elm.Attributes())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var purpose = attr.Value;
                    if (!string.IsNullOrEmpty(purpose) && !added.Contains(attr.Value))
                    {
                        var name = purpose;
                        if (names.TryGetValue(purpose, out var nameID))
                        {
                            name = _resolver.Resolve(nameID);
                        }

                        yield return new Purpose(purpose, name);
                        added.Add(purpose);
                    }
                }
            }
        }

        progress?.Report((currentStep++, maxSteps));
    }
}

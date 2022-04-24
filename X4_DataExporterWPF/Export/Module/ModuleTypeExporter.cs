using Dapper;
using LibX4.FileSystem;
using LibX4.Lang;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export;

/// <summary>
/// モジュール種別抽出用クラス
/// </summary>
public class ModuleTypeExporter : IExporter
{
    /// <summary>
    /// catファイルオブジェクト
    /// </summary>
    private readonly ICatFile _CatFile;

    /// <summary>
    /// ウェア情報xml
    /// </summary>
    private readonly XDocument _WaresXml;

    /// <summary>
    /// 言語解決用オブジェクト
    /// </summary>
    private readonly ILanguageResolver _Resolver;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="catFile">catファイルオブジェクト</param>
    /// <param name="waresXml">ウェア情報xml</param>
    /// <param name="resolver">言語解決用オブジェクト</param>
    public ModuleTypeExporter(ICatFile catFile, XDocument waresXml, LanguageResolver resolver)
    {
        _CatFile  = catFile;
        _WaresXml = waresXml;
        _Resolver = resolver;
    }



    /// <summary>
    /// 抽出処理
    /// </summary>
    /// <param name="connection"></param>
    public void Export(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            connection.Execute(@"
CREATE TABLE IF NOT EXISTS ModuleType
(
    ModuleTypeID    TEXT    NOT NULL PRIMARY KEY,
    Name            TEXT    NOT NULL
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecords(progress);

            connection.Execute("INSERT INTO ModuleType(ModuleTypeID, Name) VALUES (@ModuleTypeID, @Name)", items);
        }
    }


    /// <summary>
    /// ModuleType データを読み出す
    /// </summary>
    /// <returns>EquipmentType データ</returns>
    private IEnumerable<ModuleType> GetRecords(IProgress<(int currentStep, int maxSteps)> progress)
    {
        // 可能ならファイルから抽出したいが、ModuleTypeID と対応するテキストを紐付けるファイルが(多分)無いからこれが限界な気がする
        // 参考: "\ui\addons\ego_gameoptions\customgame.lua"
        var names = new Dictionary<string, string>
        {
            {"buildmodule",         "{1001,    2439}"},
            {"connectionmodule",    "{20104,  59901}"},
            {"defencemodule",       "{1001,    2424}"},
            {"dockarea",            "{20104,  70001}"},
            {"habitation",          "{1001,    2451}"},
            {"pier",                "{20104,  71101}"},
            {"production",          "{1001,    2421}"},
            {"storage",             "{1001,    2422}"},
            {"ventureplatform",     "{20104, 101901}"},
            {"processingmodule",    "{1001,    9621}"},
            {"welfaremodule",       "{1001,    9620}"},
        };

        var maxSteps = (int)(double)_WaresXml.Root.XPathEvaluate("count(ware[contains(@tags, 'module')])");
        var currentStep = 0;

        var added = new HashSet<string>();

        foreach (var module in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'module')]"))
        {
            progress?.Report((currentStep++, maxSteps));

            var moduleID = module.Attribute("id").Value;
            if (string.IsNullOrEmpty(moduleID)) continue;

            var macroName = module.XPathSelectElement("component").Attribute("ref").Value;
            if (string.IsNullOrEmpty(macroName)) continue;

            var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);
            var moduleTypeID = macroXml.Root.XPathSelectElement("macro").Attribute("class").Value;
            if (string.IsNullOrEmpty(moduleTypeID) || added.Contains(moduleTypeID)) continue;

            // モジュール種別 ID の名称を表すキーの取得を試みる
            if (names.TryGetValue(moduleTypeID, out var name))
            {
                name = _Resolver.Resolve(name);
            }
            else
            {
                // 未知の ModuleTypeID の場合は仕方ないので ModuleTypeID を Name として扱う
                name = moduleTypeID;
            }

            yield return new ModuleType(moduleTypeID, name);
            added.Add(moduleTypeID);
        }

        progress?.Report((currentStep++, maxSteps));
    }
}

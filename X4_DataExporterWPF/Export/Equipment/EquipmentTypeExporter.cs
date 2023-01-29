using Dapper;
using LibX4.Lang;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;
using X4_DataExporterWPF.Internal;

namespace X4_DataExporterWPF.Export;

/// <summary>
/// 装備種別情報抽出用クラス
/// </summary>
public class EquipmentTypeExporter : IExporter
{
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
    /// <param name="waresXml">ウェア情報xml</param>
    /// <param name="resolver">言語解決用オブジェクト</param>
    public EquipmentTypeExporter(XDocument waresXml, LanguageResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(waresXml.Root);

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
CREATE TABLE IF NOT EXISTS EquipmentType
(
    EquipmentTypeID TEXT    NOT NULL PRIMARY KEY,
    Name            TEXT    NOT NULL
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecords(progress);

            await connection.ExecuteAsync("INSERT INTO EquipmentType (EquipmentTypeID, Name) VALUES (@EquipmentTypeID, @Name)", items);
        }
    }


    /// <summary>
    /// EquipmentType データを読み出す
    /// </summary>
    /// <returns>EquipmentType データ</returns>
    private IEnumerable<EquipmentType> GetRecords(IProgress<(int currentStep, int maxSteps)> progress)
    {
        // TODO: 可能ならファイルから抽出する
        var names = new Dictionary<string, string>
        {
            {"countermeasures",     "{20215, 1701}"},
            {"drones",              "{20215, 1601}"},
            {"engines",             "{20215, 1801}"},
            {"missiles",            "{20215, 1901}"},
            {"shields",             "{20215, 2001}"},
            {"software",            "{20215, 2101}"},
            {"thrusters",           "{20215, 2201}"},
            {"turrets",             "{20215, 2301}"},
            {"weapons",             "{20215, 2401}"},
        };

        var maxSteps = (int)(double)_waresXml.Root!.XPathEvaluate("count(ware[@transport='equipment'])");
        var currentStep = 0;
        var added = new HashSet<string>();

        foreach (var equipment in _waresXml.Root!.XPathSelectElements("ware[@transport='equipment']"))
        {
            progress?.Report((currentStep++, maxSteps));

            var equipmentTypeID = equipment.Attribute("group")?.Value;
            if (string.IsNullOrEmpty(equipmentTypeID) || added.Contains(equipmentTypeID)) continue;

            var name = equipmentTypeID;
            if (names.TryGetValue(equipmentTypeID, out var nameID))
            {
                name = _resolver.Resolve(nameID);
            }

            yield return new EquipmentType(equipmentTypeID, name);
            added.Add(equipmentTypeID);
        }

        progress?.Report((currentStep++, maxSteps));
    }
}

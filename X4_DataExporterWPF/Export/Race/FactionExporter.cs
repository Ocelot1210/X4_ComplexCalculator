using Dapper;
using LibX4.FileSystem;
using LibX4.Lang;
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
/// 派閥情報抽出用クラス
/// </summary>
public class FactionExporter : IExporter
{
    /// <summary>
    /// catファイルオブジェクト
    /// </summary>
    private readonly CatFile _catFile;


    /// <summary>
    /// 言語解決用オブジェクト
    /// </summary>
    private readonly ILanguageResolver _resolver;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="catFile">catファイルオブジェクト</param>
    /// <param name="resolver">言語解決用オブジェクト</param>
    public FactionExporter(CatFile catFile, ILanguageResolver resolver)
    {
        _catFile = catFile;
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
CREATE TABLE IF NOT EXISTS Faction
(
    FactionID   TEXT    NOT NULL PRIMARY KEY,
    Name        TEXT    NOT NULL,
    RaceID      TEXT    NOT NULL,
    ShortName   TEXT    NOT NULL,
    Description TEXT    NOT NULL,
    Color       INTEGER,
    Icon        BLOB,
    FOREIGN KEY (RaceID)   REFERENCES Race(RaceID)
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecordsAsync(progress, cancellationToken);

            await connection.ExecuteAsync("INSERT INTO Faction (FactionID, Name, RaceID, ShortName, Description, Color, Icon) VALUES (@FactionID, @Name, @RaceID, @ShortName, @Description, @Color, @Icon)", items);
        }
    }


    /// <summary>
    /// XML から Faction データを読み出す
    /// </summary>
    /// <returns>読み出した Faction データ</returns>
    private async IAsyncEnumerable<Faction> GetRecordsAsync(IProgress<(int currentStep, int maxSteps)> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var factionsXml = await _catFile.OpenXmlAsync("libraries/factions.xml", cancellationToken);
        if (factionsXml?.Root is null) yield break;

        var maxSteps = (int)(double)factionsXml.Root.XPathEvaluate("count(faction[@name])");
        var currentStep = 0;

        foreach (var faction in factionsXml.Root.XPathSelectElements("faction[@name]"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report((currentStep++, maxSteps));

            var factionID = faction.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(factionID)) continue;

            var name = _resolver.Resolve(faction.Attribute("name")?.Value ?? "");
            if (string.IsNullOrEmpty(name)) continue;

            var raceID = faction.Attribute("primaryrace")?.Value ?? "";
            var shortName = _resolver.Resolve(faction.Attribute("shortname")?.Value ?? "");

            yield return new Faction(
                factionID,
                name,
                raceID,
                shortName,
                _resolver.Resolve(faction.Attribute("description")?.Value ?? ""),
                GetFactionColor(faction),
                await Util.DDS2PngAsync(_catFile, "assets/fx/gui/textures/factions", faction.Element("icon")?.Attribute("active")?.Value, cancellationToken)
            );
        }

        progress.Report((currentStep++, maxSteps));
    }


    /// <summary>
    /// 派閥の色を取得する
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    private static int GetFactionColor(XElement element)
    {
        var colorElm = element.Element("color");
        if (colorElm is null) return 0;

        var r = colorElm.Attribute("r")?.GetInt() ?? 0;
        var g = colorElm.Attribute("g")?.GetInt() ?? 0;
        var b = colorElm.Attribute("b")?.GetInt() ?? 0;

        return System.Drawing.Color.FromArgb(255, r, g, b).ToArgb();
    }
}

using Dapper;
using LibX4.FileSystem;
using LibX4.Xml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;
using X4_DataExporterWPF.Internal;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 装備情報抽出用クラス
    /// </summary>
    class EquipmentExporter : IExporter
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
        /// サムネ画像管理クラス
        /// </summary>
        private readonly ThumbnailManager _ThumbnailManager;


        /// <summary>
        /// 装備のタグ一覧
        /// </summary>
        private readonly LinkedList<IReadOnlyList<EquipmentTag>> _EquipmentTags = new();


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="waresXml">ウェア情報xml</param>
        public EquipmentExporter(ICatFile catFile, XDocument waresXml)
        {
            _CatFile = catFile;
            _WaresXml = waresXml;
            _ThumbnailManager = new(catFile, "assets/fx/gui/textures/upgrades", "notfound");
        }


        /// <inheritdoc/>
        public async Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
        {
            //////////////////
            // テーブル作成 //
            //////////////////
            {
                await connection.ExecuteAsync(@"
CREATE TABLE IF NOT EXISTS Equipment
(
    EquipmentID     TEXT    NOT NULL PRIMARY KEY,
    MacroName       TEXT    NOT NULL,
    EquipmentTypeID TEXT    NOT NULL,
    Hull            INTEGER NOT NULL,
    HullIntegrated  BOOLEAN NOT NULL,
    Mk              INTEGER NOT NULL,
    MakerRace       TEXT,
    Thumbnail       BLOB,
    FOREIGN KEY (EquipmentID)       REFERENCES Ware(WareID),
    FOREIGN KEY (EquipmentTypeID)   REFERENCES EquipmentType(EquipmentTypeID),
    FOREIGN KEY (MakerRace)         REFERENCES Race(RaceID)
) WITHOUT ROWID");


                await connection.ExecuteAsync(@"
CREATE TABLE IF NOT EXISTS EquipmentTag
(
    EquipmentID     TEXT    NOT NULL,
    Tag             TEXT    NOT NULL,
    FOREIGN KEY (EquipmentID)       REFERENCES Equipment(EquipmentID)
)");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecordsAsync(progress, cancellationToken);

                await connection.ExecuteAsync(@"
INSERT INTO Equipment ( EquipmentID,  MacroName,  EquipmentTypeID,  Hull,  HullIntegrated,  Mk,  MakerRace,  Thumbnail)
            VALUES    (@EquipmentID, @MacroName, @EquipmentTypeID, @Hull, @HullIntegrated, @Mk, @MakerRace, @Thumbnail)", items);

                await connection.ExecuteAsync("INSERT INTO EquipmentTag (EquipmentID, Tag) VALUES (@EquipmentID, @Tag)", _EquipmentTags.SelectMany(x => x));
            }
        }


        /// <summary>
        /// XML から Equipment データを読み出す
        /// </summary>
        /// <returns>読み出した Equipment データ</returns>
        private async IAsyncEnumerable<Equipment> GetRecordsAsync(IProgress<(int currentStep, int maxSteps)> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var maxSteps = (int)(double)_WaresXml.Root.XPathEvaluate("count(ware[@transport='equipment'])");
            var currentStep = 0;


            foreach (var equipment in _WaresXml.Root.XPathSelectElements("ware[@transport='equipment']"))
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report((currentStep++, maxSteps));

                var equipmentID = equipment.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(equipmentID)) continue;

                var macroName = equipment.XPathSelectElement("component")?.Attribute("ref")?.Value;
                if (string.IsNullOrEmpty(macroName)) continue;

                var equipmentTypeID = equipment.Attribute("group")?.Value;
                if (string.IsNullOrEmpty(equipmentTypeID)) continue;

                
                var macroXml = await _CatFile.OpenIndexXmlAsync("index/macros.xml", macroName, cancellationToken);
                XDocument componentXml;
                try
                {
                    componentXml = await _CatFile.OpenIndexXmlAsync("index/components.xml", macroXml.Root.XPathSelectElement("macro/component").Attribute("ref").Value, cancellationToken);
                }
                catch
                {
                    continue;
                }

                // 装備が記載されているタグを取得する
                var component = componentXml.Root.XPathSelectElement("component/connections/connection[contains(@tags, 'component')]");

                // タグがあれば格納する
                var tags = Util.SplitTags(component?.Attribute("tags")?.Value).Distinct();
                if (tags.Any())
                {
                    _EquipmentTags.AddLast(tags.Select(x => new EquipmentTag(equipmentID, x)).ToArray());
                }

                var idElm = macroXml.Root.XPathSelectElement("macro/properties/identification");
                if (idElm is null) continue;

                yield return new Equipment(
                    equipmentID,
                    macroName,
                    equipmentTypeID,
                    macroXml.Root.XPathSelectElement("macro/properties/hull")?.Attribute("max")?.GetInt() ?? 0,
                    (macroXml.Root.XPathSelectElement("macro/properties/hull")?.Attribute("integrated")?.GetInt() ?? 0) == 1,
                    idElm.Attribute("mk")?.GetInt() ?? 0,
                    idElm.Attribute("makerrace")?.Value,
                    await _ThumbnailManager.GetThumbnailAsync(macroName, cancellationToken)
                );
            }

            progress?.Report((currentStep++, maxSteps));
        }
    }
}

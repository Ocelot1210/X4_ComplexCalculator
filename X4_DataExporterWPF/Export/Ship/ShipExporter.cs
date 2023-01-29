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
/// 艦船情報抽出用クラス
/// </summary>
public class ShipExporter : IExporter
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
    /// サムネ画像管理クラス
    /// </summary>
    private readonly ThumbnailManager _thumbnailManager;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="catFile">catファイルオブジェクト</param>
    /// <param name="waresXml">ウェア情報xml</param>
    public ShipExporter(ICatFile catFile, XDocument waresXml)
    {
        ArgumentNullException.ThrowIfNull(waresXml.Root);

        _catFile = catFile;
        _waresXml = waresXml;
        _thumbnailManager = new(catFile, "assets/fx/gui/textures/ships", "notfound");
    }


    /// <inheritdoc/>
    public async Task ExportAsync(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress, CancellationToken cancellationToken)
    {
        //////////////////
        // テーブル作成 //
        //////////////////
        {
            await connection.ExecuteAsync(@"
CREATE TABLE IF NOT EXISTS Ship
(
    ShipID          TEXT    NOT NULL PRIMARY KEY,
    ShipTypeID      TEXT    NOT NULL,
    Macro           TEXT    NOT NULL,
    SizeID          TEXT    NOT NULL,
    Mass            REAL    NOT NULL,
    DragForward     REAL    NOT NULL,
    DragReverse     REAL    NOT NULL,
    DragHorizontal  REAL    NOT NULL,
    DragVertical    REAL    NOT NULL,
    DragPitch       REAL    NOT NULL,
    DragYaw         REAL    NOT NULL,
    DragRoll        REAL    NOT NULL,
    InertiaPitch    REAL    NOT NULL,
    InertiaYaw      REAL    NOT NULL,
    InertiaRoll     REAL    NOT NULL,
    Hull            INTEGER NOT NULL,
    People          INTEGER NOT NULL,
    MissileStorage  INTEGER NOT NULL,
    DroneStorage    INTEGER NOT NULL,
    CargoSize       INTEGER NOT NULL,
    Thumbnail       BLOB,
    FOREIGN KEY (ShipID)        REFERENCES Ware(WareID),
    FOREIGN KEY (ShipTypeID)    REFERENCES ShipType(ShipTypeID),
    FOREIGN KEY (SizeID)        REFERENCES Size(SizeID)
) WITHOUT ROWID");
        }


        ////////////////
        // データ抽出 //
        ////////////////
        {
            var items = GetRecordsAsync(progress, cancellationToken);

            await connection.ExecuteAsync(@"
INSERT INTO
Ship   ( ShipID,  ShipTypeID,  Macro,  SizeID,  Mass,  DragForward,  DragReverse,  DragHorizontal,  DragVertical,  DragPitch,  DragYaw,  DragRoll,  InertiaPitch,  InertiaYaw,  InertiaRoll,  Hull,  People,  MissileStorage,  DroneStorage,  CargoSize,  Thumbnail) 
VALUES (@ShipID, @ShipTypeID, @Macro, @SizeID, @Mass, @DragForward, @DragReverse, @DragHorizontal, @DragVertical, @DragPitch, @DragYaw, @DragRoll, @InertiaPitch, @InertiaYaw, @InertiaRoll, @Hull, @People, @MissileStorage, @DroneStorage, @CargoSize, @Thumbnail)",
items);
        }
    }


    /// <summary>
    /// レコード抽出
    /// </summary>
    private async IAsyncEnumerable<Ship> GetRecordsAsync(IProgress<(int currentStep, int maxSteps)> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var maxSteps = (int)(double)_waresXml.Root!.XPathEvaluate("count(ware[contains(@tags, 'ship')])");
        var currentStep = 0;


        foreach (var ship in _waresXml.Root!.XPathSelectElements("ware[contains(@tags, 'ship')]"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report((currentStep++, maxSteps));


            var shipID = ship.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(shipID)) continue;

            var macroName = ship.XPathSelectElement("component")?.Attribute("ref")?.Value;
            if (string.IsNullOrEmpty(macroName)) continue;

            var macroXml = await _catFile.OpenIndexXmlAsync("index/macros.xml", macroName, cancellationToken);

            var shipSizeID = GetShipSizeID(macroXml);
            if (string.IsNullOrEmpty(shipSizeID)) continue;

            var property = GetProperty(macroXml);
            if (property is null) continue;

            var cargoSize = await GetCargoSizeAsync(macroXml, cancellationToken);
            if (cargoSize < 0) continue;

            yield return new Ship(
                shipID,
                property.ShipTypeID,
                macroName,
                shipSizeID,
                property.Mass,
                property.DragForward,
                property.DragReverse,
                property.DragHorizontal,
                property.DragVertical,
                property.DragPitch,
                property.DragYaw,
                property.DragRoll,
                property.InertiaPitch,
                property.InertiaYaw,
                property.InertiaRoll,
                property.Hull,
                property.People,
                property.MissileStorage,
                property.DroneStorage,
                cargoSize,
                await _thumbnailManager.GetThumbnailAsync(macroName, cancellationToken)
            );
        }

        progress.Report((currentStep++, maxSteps));
    }


    /// <summary>
    /// 艦船サイズIDを取得する
    /// </summary>
    /// <param name="macroXml">マクロxml</param>
    /// <returns>該当するサイズID　該当なしならnull</returns>
    private static string? GetShipSizeID(XDocument macroXml)
    {
        var shipClass = macroXml.Root?.Element("macro")?.Attribute("class")?.Value ?? "";

        return shipClass switch
        {
            "ship_xs" => "extrasmall",
            "ship_s" => "small",
            "ship_m" => "medium",
            "ship_l" => "large",
            "ship_xl" => "extralarge",
            _ => null
        };
    }


    /// <summary>
    /// カーゴサイズを取得する
    /// </summary>
    /// <param name="macroXml">マクロxml</param>
    /// <returns>該当艦船のカーゴサイズ</returns>
    private async Task<int> GetCargoSizeAsync(XDocument macroXml, CancellationToken cancellationToken)
    {
        var componentName = macroXml.Root?.XPathSelectElement("macro/component")?.Attribute("ref")?.Value ?? "";
        if (string.IsNullOrEmpty(componentName)) return -1;

        var componentXml = await _catFile.OpenIndexXmlAsync("index/components.xml", componentName, cancellationToken);
        if (componentXml is null) return -1;

        var connName = componentXml.Root?.XPathSelectElement("component/connections/connection[contains(@tags, 'storage')]")?.Attribute("name")?.Value ?? "";
        if (string.IsNullOrEmpty(connName)) return 0;

        var storage = macroXml.Root?.XPathSelectElement($"macro/connections/connection[@ref='{connName}']/macro")?.Attribute("ref")?.Value ?? "";
        if (string.IsNullOrEmpty(storage))
        {
            // カーゴが無い船(ゼノンの艦船等)を考慮
            return 0;
        }


        var storageXml = await _catFile.OpenIndexXmlAsync("index/macros.xml", storage, cancellationToken);
        if (storageXml is null) return -1;

        return storageXml.Root?.XPathSelectElement("macro/properties/cargo")?.Attribute("max").GetInt() ?? 0;
    }


    /// <summary>
    /// propertiesタグ内の情報を取得する
    /// </summary>
    /// <param name="macroXml"></param>
    /// <returns></returns>
    private static ShipProperty? GetProperty(XDocument macroXml)
    {
        var properties = macroXml.Root?.XPathSelectElement("macro/properties");
        if (properties is null) return null;

        // 艦船種別IDを取得
        var shipTypeId = properties.Element("ship")?.Attribute("type")?.Value;
        if (string.IsNullOrEmpty(shipTypeId)) return null;

        // 抗力を取得
        var drag = properties.XPathSelectElement("physics/drag");
        if (drag is null) return null;

        // 慣性を取得
        var inertia = properties.XPathSelectElement("physics/inertia");
        if (inertia is null) return null;

        var ret = new ShipProperty
        {
            ShipTypeID      = shipTypeId,
            DragForward     = drag.Attribute("forward")?.GetDouble()    ?? 0.0,
            DragReverse     = drag.Attribute("reverse")?.GetDouble()    ?? 0.0,
            DragHorizontal  = drag.Attribute("horizontal")?.GetDouble() ?? 0.0,
            DragVertical    = drag.Attribute("vertical")?.GetDouble()   ?? 0.0,
            DragPitch       = drag.Attribute("pitch")?.GetDouble()      ?? 0.0,
            DragYaw         = drag.Attribute("yaw")?.GetDouble()        ?? 0.0,
            DragRoll        = drag.Attribute("roll")?.GetDouble()       ?? 0.0,

            InertiaPitch    = inertia.Attribute("pitch")?.GetDouble()   ?? 0.0,
            InertiaYaw      = inertia.Attribute("yaw")?.GetDouble()     ?? 0.0,
            InertiaRoll     = inertia.Attribute("roll")?.GetDouble()    ?? 0.0,

            Hull            = properties.Element("hull")?.Attribute("max").GetInt()         ?? 0,
            Mass            = properties.Element("physics")?.Attribute("mass").GetDouble()  ?? 0,
            People          = properties.Element("people")?.Attribute("capacity").GetInt()  ?? 0,
            MissileStorage  = properties.Element("storage")?.Attribute("missile")?.GetInt() ?? 0,       // ミサイル搭載不能なら搭載量は0
            DroneStorage    = properties.Element("storage")?.Attribute("unit")?.GetInt()    ?? 0        // ドローン搭載不能なら搭載量は0
        };

        return ret;
    }


    private class ShipProperty
    {
        public string ShipTypeID = "";
        public double Mass;
        public double DragForward;
        public double DragReverse;
        public double DragHorizontal;
        public double DragVertical;
        public double DragPitch;
        public double DragYaw;
        public double DragRoll;
        public double InertiaPitch;
        public double InertiaYaw;
        public double InertiaRoll;
        public int Hull;
        public int People;
        public int MissileStorage;
        public int DroneStorage;
    }
}

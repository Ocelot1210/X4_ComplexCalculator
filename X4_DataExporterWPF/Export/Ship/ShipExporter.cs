using Dapper;
using LibX4.FileSystem;
using LibX4.Xml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 艦船情報抽出用クラス
    /// </summary>
    public class ShipExporter : IExporter
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
        /// サムネ画像が見つからなかった場合の画像
        /// </summary>
        private byte[]? _NotFoundThumbnail;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="waresXml">ウェア情報xml</param>
        public ShipExporter(CatFile catFile, XDocument waresXml)
        {
            _CatFile = catFile;
            _WaresXml = waresXml;
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
                var items = GetRecords(progress);

                connection.Execute(@"
INSERT INTO
Ship   ( ShipID,  ShipTypeID,  Macro,  SizeID,  Mass,  DragForward,  DragReverse,  DragHorizontal,  DragVertical,  DragPitch,  DragYaw,  DragRoll,  InertiaPitch,  InertiaYaw,  InertiaRoll,  Hull,  People,  MissileStorage,  DroneStorage,  CargoSize,  Thumbnail) 
VALUES (@ShipID, @ShipTypeID, @Macro, @SizeID, @Mass, @DragForward, @DragReverse, @DragHorizontal, @DragVertical, @DragPitch, @DragYaw, @DragRoll, @InertiaPitch, @InertiaYaw, @InertiaRoll, @Hull, @People, @MissileStorage, @DroneStorage, @CargoSize, @Thumbnail)",
items);
            }
        }


        /// <summary>
        /// レコード抽出
        /// </summary>
        private IEnumerable<Ship> GetRecords(IProgress<(int currentStep, int maxSteps)> progress)
        {
            var maxSteps = (int)(double)_WaresXml.Root.XPathEvaluate("count(ware[contains(@tags, 'ship')])");
            var currentStep = 0;


            foreach (var ship in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'ship')]"))
            {
                progress.Report((currentStep++, maxSteps));


                var shipID = ship.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(shipID)) continue;

                var macroName = ship.XPathSelectElement("component").Attribute("ref").Value;
                var macroXml = _CatFile.OpenIndexXml("index/macros.xml", macroName);

                var shipSizeID = GetShipSizeID(macroXml);
                if (string.IsNullOrEmpty(shipSizeID)) continue;

                var property = GetProperty(macroXml);
                if (property is null) continue;

                var cargoSize = GetCargoSize(macroXml);
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
                    GetThumbnail(macroName)                    
                );
            }

            progress.Report((currentStep++, maxSteps));
        }


        /// <summary>
        /// 艦船サイズIDを取得する
        /// </summary>
        /// <param name="macroXml">マクロxml</param>
        /// <returns>該当するサイズID　該当なしならnull</returns>
        private string? GetShipSizeID(XDocument macroXml)
        {
            var shipClass = macroXml.Root.Element("macro").Attribute("class").Value ?? "";

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
        private int GetCargoSize(XDocument macroXml)
        {
            var componentName = macroXml.Root.XPathSelectElement("macro/component")?.Attribute("ref")?.Value ?? "";
            if (string.IsNullOrEmpty(componentName)) return -1;

            var componentXml = _CatFile.OpenIndexXml("index/components.xml", componentName);
            if (componentXml is null) return -1;

            var connName = componentXml.Root.XPathSelectElement("component/connections/connection[contains(@tags, 'storage')]")?.Attribute("name")?.Value ?? "";
            if (string.IsNullOrEmpty(connName)) return 0;

            var storage = macroXml.Root.XPathSelectElement($"macro/connections/connection[@ref='{connName}']/macro")?.Attribute("ref")?.Value ?? "";
            if (string.IsNullOrEmpty(storage))
            {
                // カーゴが無い船(ゼノンの艦船等)を考慮
                return 0;
            }


            var storageXml = _CatFile.OpenIndexXml("index/macros.xml", storage);
            if (storageXml is null) return -1;

            return storageXml.Root.XPathSelectElement("macro/properties/cargo").Attribute("max").GetInt();
        }


        /// <summary>
        /// propertiesタグ内の情報を取得する
        /// </summary>
        /// <param name="macroXml"></param>
        /// <returns></returns>
        private ShipProperty? GetProperty(XDocument macroXml)
        {
            var properties = macroXml.Root.XPathSelectElement("macro/properties");
            if (properties is null) return null;


            var ret = new ShipProperty();
            // 説明文を取得

            // 艦船種別IDを取得
            ret.ShipTypeID = properties.Element("ship")?.Attribute("type")?.Value ?? "";
            if (string.IsNullOrEmpty(ret.ShipTypeID)) return null;


            // 抗力を取得
            {
                var drag = properties.XPathSelectElement("physics/drag");
                if (drag is null) return null;

                ret.DragForward = drag.Attribute("forward")?.GetDouble() ?? 0.0;
                ret.DragReverse = drag.Attribute("reverse")?.GetDouble() ?? 0.0;
                ret.DragHorizontal = drag.Attribute("horizontal")?.GetDouble() ?? 0.0;
                ret.DragVertical = drag.Attribute("vertical")?.GetDouble() ?? 0.0;
                ret.DragPitch = drag.Attribute("pitch")?.GetDouble() ?? 0.0;
                ret.DragYaw = drag.Attribute("yaw")?.GetDouble() ?? 0.0;
                ret.DragRoll = drag.Attribute("roll")?.GetDouble() ?? 0.0;
            }

            // 慣性を取得
            {
                var inertia = properties.XPathSelectElement("physics/inertia");
                if (inertia is null) return null;

                ret.InertiaPitch = inertia.Attribute("pitch")?.GetDouble() ?? 0.0;
                ret.InertiaYaw = inertia.Attribute("yaw")?.GetDouble() ?? 0.0;
                ret.InertiaRoll = inertia.Attribute("roll")?.GetDouble() ?? 0.0;
            }

            ret.Hull = properties.Element("hull")?.Attribute("max").GetInt() ?? 0;
            ret.Mass = properties.Element("physics")?.Attribute("mass").GetDouble() ?? 0;
            ret.People = properties.Element("people")?.Attribute("capacity").GetInt() ?? 0;
            ret.MissileStorage = properties.Element("storage")?.Attribute("missile")?.GetInt() ?? 0;    // ミサイル搭載不能なら搭載量は0
            ret.DroneStorage = properties.Element("storage")?.Attribute("unit")?.GetInt() ?? 0;         // ドローン搭載不能なら搭載量は0

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


        
        /// <summary>
        /// サムネ画像を取得する
        /// </summary>
        /// <param name="macroName">マクロ名</param>
        /// <returns>サムネ画像のbyte配列</returns>
        private byte[]? GetThumbnail(string macroName)
        {
            const string dir = "assets/fx/gui/textures/ships";

            var ret = Util.DDS2Png(_CatFile, dir, macroName);
            if (ret is not null)
            {
                return ret;
            }

            if (_NotFoundThumbnail is null)
            {
                _NotFoundThumbnail = Util.DDS2Png(_CatFile, dir, "notfound");
            }

            return _NotFoundThumbnail;
        }
    }
}

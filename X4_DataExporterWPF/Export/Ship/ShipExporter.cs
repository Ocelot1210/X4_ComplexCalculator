using Dapper;
using LibX4.FileSystem;
using LibX4.Lang;
using LibX4.Xml;
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
        /// 言語解決用オブジェクト
        /// </summary>
        private readonly ILanguageResolver _Resolver;

        /// <summary>
        /// サムネ画像が見つからなかった場合の画像
        /// </summary>
        private byte[]? _NotFoundThumbnail;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="waresXml">ウェア情報xml</param>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public ShipExporter(CatFile catFile, XDocument waresXml, ILanguageResolver resolver)
        {
            _CatFile = catFile;
            _WaresXml = waresXml;
            _Resolver = resolver;
        }


        /// <summary>
        /// 抽出処理
        /// </summary>
        /// <param name="connection"></param>
        public void Export(IDbConnection connection)
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
    Name            TEXT    NOT NULL,
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
    MinPrice        INTEGER NOT NULL,
    AvgPrice        INTEGER NOT NULL,
    MaxPrice        INTEGER NOT NULL,
    Description     TEXT    NOT NULL,
    Thumbnail       BLOB,
    FOREIGN KEY (ShipTypeID)        REFERENCES ShipType(ShipTypeID),
    FOREIGN KEY (SizeID)            REFERENCES Size(SizeID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute(@"
INSERT INTO
Ship   ( ShipID,  ShipTypeID,  Name,  Macro,  SizeID,  Mass,  DragForward,  DragReverse,  DragHorizontal,  DragVertical,  DragPitch,  DragYaw,  DragRoll,  InertiaPitch,  InertiaYaw,  InertiaRoll,  Hull,  People,  MissileStorage,  DroneStorage,  CargoSize,  MinPrice,  AvgPrice,  MaxPrice,  Description,  Thumbnail) 
VALUES (@ShipID, @ShipTypeID, @Name, @Macro, @SizeID, @Mass, @DragForward, @DragReverse, @DragHorizontal, @DragVertical, @DragPitch, @DragYaw, @DragRoll, @InertiaPitch, @InertiaYaw, @InertiaRoll, @Hull, @People, @MissileStorage, @DroneStorage, @CargoSize, @MinPrice, @AvgPrice, @MaxPrice, @Description, @Thumbnail)",
items);
            }
        }


        /// <summary>
        /// レコード抽出
        /// </summary>
        private IEnumerable<Ship> GetRecords()
        {
            foreach (var ship in _WaresXml.Root.XPathSelectElements("ware[contains(@tags, 'ship')]"))
            {
                var shipID = ship.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(shipID)) continue;

                var shipName = _Resolver.Resolve(ship.Attribute("name")?.Value ?? "");
                if (string.IsNullOrEmpty(shipName)) continue;

                var price = ship.Element("price");
                if (price is null) continue;


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
                    shipName, macroName,
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
                    price.Attribute("min").GetInt(),
                    price.Attribute("average").GetInt(),
                    price.Attribute("max").GetInt(),
                    property.Description,
                    GetThumbnail(macroName)                    
                );
            }
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
            if (string.IsNullOrEmpty(connName)) return -1;

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
            ret.Description = _Resolver.Resolve(properties.Element("identification").Attribute("description")?.Value ?? "");

            // 艦船種別IDを取得
            ret.ShipTypeID = properties.Element("ship").Attribute("type")?.Value ?? "";
            if (string.IsNullOrEmpty(ret.ShipTypeID)) return null;


            // 抗力を取得
            {
                var drag = properties.XPathSelectElement("physics/drag");
                if (drag is null) return null;

                ret.DragForward = drag.Attribute("forward").GetDouble();
                ret.DragReverse = drag.Attribute("reverse").GetDouble();
                ret.DragHorizontal = drag.Attribute("horizontal").GetDouble();
                ret.DragVertical = drag.Attribute("vertical").GetDouble();
                ret.DragPitch = drag.Attribute("pitch").GetDouble();
                ret.DragYaw = drag.Attribute("yaw").GetDouble();
                ret.DragRoll = drag.Attribute("roll").GetDouble();
            }

            // 慣性を取得
            {
                var inertia = properties.XPathSelectElement("physics/inertia");
                if (inertia is null) return null;

                ret.InertiaPitch = inertia.Attribute("pitch").GetDouble();
                ret.InertiaYaw = inertia.Attribute("yaw").GetDouble();
                ret.InertiaRoll = inertia.Attribute("roll").GetDouble();
            }

            ret.Hull = properties.Element("hull").Attribute("max").GetInt();
            ret.Mass = properties.Element("physics").Attribute("mass").GetDouble();
            ret.People = properties.Element("people").Attribute("capacity").GetInt();
            ret.MissileStorage = properties.Element("storage")?.Attribute("missile")?.GetInt() ?? 0;    // ミサイル搭載不能なら搭載量は0
            ret.DroneStorage = properties.Element("storage")?.Attribute("unit")?.GetInt() ?? 0;         // ドローン搭載不能なら搭載量は0

            return ret;
        }


        private class ShipProperty
        {
            public string ShipTypeID = "";
            public string Description = "";
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
            var ret = Util.GzDds2Png(_CatFile, "assets/fx/gui/textures/ships", macroName);
            if (ret is not null)
            {
                return ret;
            }

            if (_NotFoundThumbnail is null)
            {
                _NotFoundThumbnail = Util.GzDds2Png(_CatFile, "assets/fx/gui/textures/ships", "notfound.gz");
            }

            return _NotFoundThumbnail;
        }
    }
}

using Dapper;
using LibX4.Lang;
using System.Collections.Generic;
using System.Data;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 艦船種別抽出用クラス
    /// </summary>
    public class ShipTypeExporter : IExporter
    {
        /// <summary>
        /// 言語解決用オブジェクト
        /// </summary>
        private readonly ILanguageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public ShipTypeExporter(LanguageResolver resolver)
        {
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
CREATE TABLE IF NOT EXISTS ShipType
(
    ShipTypeID  TEXT    NOT NULL PRIMARY KEY,
    Name        TEXT    NOT NULL,
    Description TEXT    NOT NULL
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute("INSERT INTO ShipType (ShipTypeID, Name, Description) VALUES (@ShipTypeID, @Name, @Description)", items);
            }
        }


        private IEnumerable<ShipType> GetRecords()
        {
            // TODO: 可能ならファイルから抽出する

            //////////
            // 特小 //
            //////////
            yield return CreateShipType("personalvehicle",  1001, 1002);    // 個人乗用船
            yield return CreateShipType("police",           1011, 1012);    // 警察船
            yield return CreateShipType("xsdrone",          1021, 1022);    // ドローン
            yield return CreateShipType("escapepod",        1031, 1032);    // 脱出ポッド
            yield return CreateShipType("lasertower",       1041, 1042);    // レーザータワー
            yield return CreateShipType("distressdrone",    1051, 1052);    // ドローン


            //////////
            // 小型 //
            //////////
            yield return CreateShipType("scout",            2001, 2002);    // 偵察機
            yield return CreateShipType("fighter",          2011, 2012);    // 戦闘機
            yield return CreateShipType("heavyfighter",     2021, 2022);    // 重戦闘機
            yield return CreateShipType("interceptor",      2031, 2032);    // 要撃機
            yield return CreateShipType("courier",          2041, 2042);    // 配達船
            yield return CreateShipType("smalldrone",       2051, 2052);    // ドローン


            //////////
            // 中型 //
            //////////
            yield return CreateShipType("bomber",           3001, 3002);    // 爆撃機
            yield return CreateShipType("frigate",          3011, 3012);    // フリゲート
            yield return CreateShipType("corvette",         3021, 3022);    // コルベット
            yield return CreateShipType("transporter",      3031, 3032);    // 輸送船
            yield return CreateShipType("miner",            3041, 3042);    // 採掘船
            //yield return CreateShipType("",                 3051, 3052);    // ■人員輸送船 (ID不明)
            yield return CreateShipType("scavenger",        3061, 3062);    // 廃品回収船
            yield return CreateShipType("gunboat",          5041, 5042);    // 砲艦

            //////////
            // 大型 //
            //////////
            yield return CreateShipType("destroyer",        4001, 4002);    // 駆逐艦
            yield return CreateShipType("largeminer",       4011, 4012);    // 採掘船
            yield return CreateShipType("freighter",        4021, 4022);    // 貨物船


            ////////////
            // 特大型 //
            ////////////
            yield return CreateShipType("carrier",          5001, 5002);    // 空母
            yield return CreateShipType("resupplier",       5011, 5012);    // 補助艦船
            yield return CreateShipType("builder",          5021, 5022);    // 建築船
            yield return CreateShipType("battleship",       5031, 5032);    // 戦艦
        }


        /// <summary>
        /// 艦船種別作成
        /// </summary>
        /// <param name="id">艦船種別ID</param>
        /// <param name="name">名称のキー</param>
        /// <param name="description">説明文のキー</param>
        /// <returns></returns>
        private ShipType CreateShipType(string id, int name, int description)
        {
            return new ShipType(id, _Resolver.Resolve($"{{20221, {name}}}"), _Resolver.Resolve($"{{20221, {description}}}"));
        }
    }
}

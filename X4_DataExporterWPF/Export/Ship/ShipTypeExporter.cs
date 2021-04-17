using Dapper;
using LibX4.Lang;
using System;
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
        public void Export(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress)
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
                var items = GetRecords(progress);

                connection.Execute("INSERT INTO ShipType (ShipTypeID, Name, Description) VALUES (@ShipTypeID, @Name, @Description)", items);
            }
        }


        private IEnumerable<ShipType> GetRecords(IProgress<(int currentStep, int maxSteps)> progress)
        {
            // TODO: 可能ならファイルから抽出する

            (string id, int name, int descr)[] data =
            {
                // 特小
                ("personalvehicle",  1001, 1002),       // 個人乗用船
                ("police",           1011, 1012),       // 警察船
                ("xsdrone",          1021, 1022),       // ドローン
                ("escapepod",        1031, 1032),       // 脱出ポッド
                ("lasertower",       1041, 1042),       // レーザータワー
                ("distressdrone",    1051, 1052),       // ドローン

                // 小型
                ("scout",            2001, 2002),       // 偵察機
                ("fighter",          2011, 2012),       // 戦闘機
                ("heavyfighter",     2021, 2022),       // 重戦闘機
                ("interceptor",      2031, 2032),       // 要撃機
                ("courier",          2041, 2042),       // 配達船
                ("smalldrone",       2051, 2052),       // ドローン

                // 中型
                ("bomber",           3001, 3002),       // 爆撃機
                ("frigate",          3011, 3012),       // フリゲート
                ("corvette",         3021, 3022),       // コルベット
                ("transporter",      3031, 3032),       // 輸送船
                ("miner",            3041, 3042),       // 採掘船
                //("",                 3051, 3052),       // ■人員輸送船 (ID不明)
                ("scavenger",        3061, 3062),       // 廃品回収船
                ("gunboat",          5041, 5042),       // 砲艦

                // 大型
                ("destroyer",        4001, 4002),       // 駆逐艦
                ("freighter",        4011, 4012),       // 貨物船
                ("largeminer",       4021, 4022),       // 採掘船

                // 特大型
                ("carrier",          5001, 5002),       // 空母
                ("resupplier",       5011, 5012),       // 補助艦船
                ("builder",          5021, 5022),       // 建築船
                ("battleship",       5031, 5032),       // 戦艦
            };

            var currentStep = 0;

            progress.Report((currentStep++, data.Length));

            foreach (var (id, name, descr) in data)
            {
                yield return new ShipType(id, _Resolver.Resolve($"{{20221, {name}}}"), _Resolver.Resolve($"{{20221, {descr}}}"));
                progress.Report((currentStep++, data.Length));
            }
        }
    }
}

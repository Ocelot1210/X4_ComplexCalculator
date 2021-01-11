using Dapper;
using LibX4.Lang;
using System.Collections.Generic;
using System.Data;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 用途抽出用クラス
    /// </summary>
    public class PurposeExporter : IExporter
    {
        /// <summary>
        /// 言語解決用オブジェクト
        /// </summary>
        private readonly ILanguageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public PurposeExporter(ILanguageResolver resolver)
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
                var items = GetRecords();

                connection.Execute("INSERT INTO Purpose (PurposeID, Name) VALUES (@PurposeID, @Name)", items);
            }
        }


        private IEnumerable<Purpose> GetRecords()
        {
            // TODO: 可能ならファイルから抽出する

            //yield return new Purpose("",            _Resolver.Resolve("{20213,  100}"));     // ■全般  (IDが不明)
            yield return new Purpose("trade",       _Resolver.Resolve("{20213,  200}"));     // 交易
            yield return new Purpose("fight",       _Resolver.Resolve("{20213,  300}"));     // 戦闘
            yield return new Purpose("build",       _Resolver.Resolve("{20213,  400}"));     // 建築
            yield return new Purpose("mine",        _Resolver.Resolve("{20213,  500}"));     // 採掘
            yield return new Purpose("hack",        _Resolver.Resolve("{20213,  600}"));     // ハッキング (IDは仮)
            yield return new Purpose("scan",        _Resolver.Resolve("{20213,  700}"));     // スキャン   (IDは仮)
            yield return new Purpose("production",  _Resolver.Resolve("{20213,  800}"));     // 製造
            yield return new Purpose("storage",     _Resolver.Resolve("{20213,  900}"));     // 保管
            yield return new Purpose("connection",  _Resolver.Resolve("{20213, 1000}"));     // 接続
            yield return new Purpose("habitation",  _Resolver.Resolve("{20213, 1100}"));     // 居住
            yield return new Purpose("defence",     _Resolver.Resolve("{20213, 1200}"));     // 防衛
            yield return new Purpose("docking",     _Resolver.Resolve("{20213, 1300}"));     // ドッキング
            yield return new Purpose("venture",     _Resolver.Resolve("{20213, 1400}"));     // 探検
            yield return new Purpose("auxiliary",   _Resolver.Resolve("{20213, 1500}"));     // 採掘
        }
    }
}

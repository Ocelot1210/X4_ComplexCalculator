using System.Collections.Generic;
using System.Data;
using Dapper;
using LibX4.Lang;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// カーゴ種別抽出用クラス
    /// </summary>
    class TransportTypeExporter : IExporter
    {
        /// <summary>
        /// 言語解決用オブジェクト
        /// </summary>
        private readonly LanguageResolver Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public TransportTypeExporter(LanguageResolver resolver)
        {
            Resolver = resolver;
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
CREATE TABLE IF NOT EXISTS TransportType
(
    TransportTypeID TEXT    NOT NULL PRIMARY KEY,
    Name            TEXT    NOT NULL
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                // レコード追加
                connection.Execute("INSERT INTO TransportType (TransportTypeID, Name) VALUES (@TransportTypeID, @Name)", items);
            }
        }


        /// <summary>
        /// XML から TransportType データを読み出す
        /// </summary>
        /// <returns>読み出した TransportType データ</returns>
        private IEnumerable<TransportType> GetRecords()
        {
            // TODO: 可能ならファイルから抽出する
            yield return new TransportType("container", Resolver.Resolve("{20205, 100}"));
            yield return new TransportType("liquid", Resolver.Resolve("{20205, 300}"));
            yield return new TransportType("solid", Resolver.Resolve("{20205, 200}"));
        }
    }
}

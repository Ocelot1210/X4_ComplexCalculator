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
        private readonly LangageResolver Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public TransportTypeExporter(LangageResolver resolver)
        {
            Resolver = resolver;
        }


        /// <summary>
        /// 抽出処理
        /// </summary>
        /// <param name="cmd"></param>
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
                // TODO: 可能ならファイルから抽出する
                TransportType[] items =
                {
                    new TransportType("container", Resolver.Resolve("{20205, 100}")),
                    new TransportType("liquid",    Resolver.Resolve("{20205, 300}")),
                    new TransportType("solid",     Resolver.Resolve("{20205, 200}"))
                };

                // レコード追加
                connection.Execute("INSERT INTO TransportType (TransportTypeID, Name) VALUES (@TransportTypeID, @Name)", items);
            }
        }
    }
}

using System.Data;
using Dapper;
using LibX4.Lang;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// サイズ情報抽出用クラス
    /// </summary>
    class SizeExporter : IExporter
    {
        /// <summary>
        /// 言語解決用オブジェクト
        /// </summary>
        private readonly LangageResolver Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public SizeExporter(LangageResolver resolver)
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
CREATE TABLE IF NOT EXISTS Size
(
    SizeID  TEXT    NOT NULL PRIMARY KEY,
    Name    TEXT    NOT NULL
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                // TODO:可能ならファイルから抽出する
                Size[] items =
                {
                    new Size("extrasmall",  Resolver.Resolve("{1001, 52}")),
                    new Size("small",       Resolver.Resolve("{1001, 51}")),
                    new Size("medium",      Resolver.Resolve("{1001, 50}")),
                    new Size("large",       Resolver.Resolve("{1001, 49}")),
                    new Size("extralarge",  Resolver.Resolve("{1001, 48}")),
                };

                connection.Execute("INSERT INTO Size (SizeID, Name) VALUES (@SizeID, @Name)", items);
            }
        }
    }
}

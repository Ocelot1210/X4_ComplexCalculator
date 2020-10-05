using System.Collections.Generic;
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
        private readonly LanguageResolver Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public SizeExporter(LanguageResolver resolver)
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
                var items = GetRecords();

                connection.Execute("INSERT INTO Size (SizeID, Name) VALUES (@SizeID, @Name)", items);
            }
        }


        /// <summary>
        /// XML から Size データを読み出す
        /// </summary>
        /// <returns>読み出した Size データ</returns>
        private IEnumerable<Size> GetRecords()
        {
            // TODO: 可能ならファイルから抽出する
            yield return new Size("extrasmall", Resolver.Resolve("{1001, 52}"));
            yield return new Size("small", Resolver.Resolve("{1001, 51}"));
            yield return new Size("medium", Resolver.Resolve("{1001, 50}"));
            yield return new Size("large", Resolver.Resolve("{1001, 49}"));
            yield return new Size("extralarge", Resolver.Resolve("{1001, 48}"));
        }
    }
}

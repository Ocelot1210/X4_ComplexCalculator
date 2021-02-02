using System;
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
        public void Export(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress)
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
                var items = GetRecords(progress);

                connection.Execute("INSERT INTO Size (SizeID, Name) VALUES (@SizeID, @Name)", items);
            }
        }


        /// <summary>
        /// XML から Size データを読み出す
        /// </summary>
        /// <returns>読み出した Size データ</returns>
        private IEnumerable<Size> GetRecords(IProgress<(int currentStep, int maxSteps)> progress)
        {
            // TODO: 可能ならファイルから抽出する
            (string id, string name)[] data =
            {
                ("extrasmall",  "{1001, 52}"),
                ("small",       "{1001, 51}"),
                ("medium",      "{1001, 50}"),
                ("large",       "{1001, 49}"),
                ("extralarge",  "{1001, 48}"),
            };

            int currentStep = 0;
            progress.Report((currentStep++, data.Length));
            foreach (var (id, name) in data)
            {
                yield return new Size(id, name);
                progress.Report((currentStep++, data.Length));
            }
        }
    }
}

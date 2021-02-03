using System;
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
        public void Export(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress)
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
                var items = GetRecords(progress);

                // レコード追加
                connection.Execute("INSERT INTO TransportType (TransportTypeID, Name) VALUES (@TransportTypeID, @Name)", items);
            }
        }


        /// <summary>
        /// XML から TransportType データを読み出す
        /// </summary>
        /// <returns>読み出した TransportType データ</returns>
        private IEnumerable<TransportType> GetRecords(IProgress<(int currentStep, int maxSteps)> progress)
        {
            // TODO: 可能ならファイルから抽出する
            (string ID, int Code)[] data =
            {
                ("container",   100),
                ("solid",       200),
                ("liquid",      300),
                ("passenger",   400),
                ("equipment",   500),
                ("inventory",   600),
                ("software",    700),
                ("workunit",    800),
                ("ship",        900),
                ("research",   1000),
            };

            int currentStep = 0;
            progress.Report((currentStep++, data.Length));
            foreach (var (id, code) in data)
            {
                yield return new TransportType(id, Resolver.Resolve($"{{20205, {code}}}"));
                progress.Report((currentStep++, data.Length));
            }
        }
    }
}

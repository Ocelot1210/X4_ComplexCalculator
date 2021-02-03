using Dapper;
using LibX4.Lang;
using System;
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
        public void Export(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progress)
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
                var items = GetRecords(progress);

                connection.Execute("INSERT INTO Purpose (PurposeID, Name) VALUES (@PurposeID, @Name)", items);
            }
        }


        private IEnumerable<Purpose> GetRecords(IProgress<(int currentStep, int maxSteps)> progress)
        {
            // TODO: 可能ならファイルから抽出する

            (string id, string name)[] data =
            {
                //("",              "{20213,  100}"),     // ■全般  (IDが不明)
                ("trade",         "{20213,  200}"),     // 交易
                ("fight",         "{20213,  300}"),     // 戦闘
                ("build",         "{20213,  400}"),     // 建築
                ("mine",          "{20213,  500}"),     // 採掘
                ("hack",          "{20213,  600}"),     // ハッキング (IDは仮)
                ("scan",          "{20213,  700}"),     // スキャン   (IDは仮)
                ("production",    "{20213,  800}"),     // 製造
                ("storage",       "{20213,  900}"),     // 保管
                ("connection",    "{20213, 1000}"),     // 接続
                ("habitation",    "{20213, 1100}"),     // 居住
                ("defence",       "{20213, 1200}"),     // 防衛
                ("docking",       "{20213, 1300}"),     // ドッキング
                ("venture",       "{20213, 1400}"),     // 探検
                ("auxiliary",     "{20213, 1500}"),     // 採掘
            };

            var currentStep = 0;
            progress.Report((currentStep++, data.Length));

            foreach (var (id, name) in data)
            {
                yield return new Purpose(id, _Resolver.Resolve(name));
                progress.Report((currentStep++, data.Length));
            }
        }
    }
}

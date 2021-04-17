using Dapper;
using LibX4.Lang;
using System;
using System.Collections.Generic;
using System.Data;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// モジュール種別抽出用クラス
    /// </summary>
    public class ModuleTypeExporter : IExporter
    {

        /// <summary>
        /// 言語解決用オブジェクト
        /// </summary>
        private readonly ILanguageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public ModuleTypeExporter(LanguageResolver resolver)
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
CREATE TABLE IF NOT EXISTS ModuleType
(
    ModuleTypeID    TEXT    NOT NULL PRIMARY KEY,
    Name            TEXT    NOT NULL
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords(progress);

                connection.Execute("INSERT INTO ModuleType(ModuleTypeID, Name) VALUES (@ModuleTypeID, @Name)", items);
            }
        }


        /// <summary>
        /// ModuleType データを読み出す
        /// </summary>
        /// <returns>EquipmentType データ</returns>
        private IEnumerable<ModuleType> GetRecords(IProgress<(int currentStep, int maxSteps)> progress)
        {
            // TODO: 可能ならファイルから抽出する
            (string id, string name)[] data =
            {
                ("buildmodule",         "{20104,  69901}"),
                ("connectionmodule",    "{20104,  59901}"),
                ("defencemodule",       "{20104,  49901}"),
                ("dockarea",            "{20104,  70001}"),
                ("habitation",          "{20104,  39901}"),
                ("pier",                "{20104,  71101}"),
                ("production",          "{20104,  19901}"),
                ("storage",             "{20104,  29901}"),
                ("ventureplatform",     "{20104, 101901}"),
            };

            var currentStep = 0;

            progress.Report((currentStep++, data.Length));

            foreach (var (id, name) in data)
            {
                yield return new ModuleType(id, _Resolver.Resolve(name));
                progress.Report((currentStep++, data.Length));
            }
        }
    }
}

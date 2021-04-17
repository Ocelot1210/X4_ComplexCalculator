using Dapper;
using LibX4.Lang;
using System;
using System.Collections.Generic;
using System.Data;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 装備種別情報抽出用クラス
    /// </summary>
    public class EquipmentTypeExporter : IExporter
    {
        /// <summary>
        /// 言語解決用オブジェクト
        /// </summary>
        private readonly ILanguageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public EquipmentTypeExporter(LanguageResolver resolver)
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
CREATE TABLE IF NOT EXISTS EquipmentType
(
    EquipmentTypeID TEXT    NOT NULL PRIMARY KEY,
    Name            TEXT    NOT NULL
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords(progress);

                connection.Execute("INSERT INTO EquipmentType (EquipmentTypeID, Name) VALUES (@EquipmentTypeID, @Name)", items);
            }
        }


        /// <summary>
        /// EquipmentType データを読み出す
        /// </summary>
        /// <returns>EquipmentType データ</returns>
        private IEnumerable<EquipmentType> GetRecords(IProgress<(int currentStep, int maxSteps)> progress)
        {
            // TODO: 可能ならファイルから抽出する
            (string id, string name)[] data =
            {
                ("countermeasures",     "{20215, 1701}"),
                ("drones",              "{20215, 1601}"),
                ("engines",             "{20215, 1801}"),
                ("missiles",            "{20215, 1901}"),
                ("shields",             "{20215, 2001}"),
                ("software",            "{20215, 2101}"),
                ("thrusters",           "{20215, 2201}"),
                ("turrets",             "{20215, 2301}"),
                ("weapons",             "{20215, 2401}"),
            };

            var currentStep = 0;

            progress.Report((currentStep++, data.Length));

            foreach (var (id, name) in data)
            {
                yield return new EquipmentType(id, _Resolver.Resolve(name));
                progress.Report((currentStep++, data.Length));
            }
        }
    }
}

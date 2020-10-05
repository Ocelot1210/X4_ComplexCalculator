using System.Collections.Generic;
using System.Data;
using Dapper;
using LibX4.Lang;
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
        public void Export(IDbConnection connection)
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
                var items = GetRecords();

                connection.Execute("INSERT INTO ModuleType(ModuleTypeID, Name) VALUES (@ModuleTypeID, @Name)", items);
            }
        }


        /// <summary>
        /// ModuleType データを読み出す
        /// </summary>
        /// <returns>EquipmentType データ</returns>
        private IEnumerable<ModuleType> GetRecords()
        {
            // TODO: 可能ならファイルから抽出する
            yield return new ModuleType("buildmodule", _Resolver.Resolve("{20104,  69901}"));
            yield return new ModuleType("connectionmodule", _Resolver.Resolve("{20104,  59901}"));
            yield return new ModuleType("defencemodule", _Resolver.Resolve("{20104,  49901}"));
            yield return new ModuleType("dockarea", _Resolver.Resolve("{20104,  70001}"));
            yield return new ModuleType("habitation", _Resolver.Resolve("{20104,  39901}"));
            yield return new ModuleType("pier", _Resolver.Resolve("{20104,  71101}"));
            yield return new ModuleType("production", _Resolver.Resolve("{20104,  19901}"));
            yield return new ModuleType("storage", _Resolver.Resolve("{20104,  29901}"));
            yield return new ModuleType("ventureplatform", _Resolver.Resolve("{20104, 101901}"));
        }
    }
}

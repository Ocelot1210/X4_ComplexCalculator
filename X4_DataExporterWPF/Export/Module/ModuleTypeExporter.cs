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
        private readonly LangageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public ModuleTypeExporter(LangageResolver resolver)
        {
            _Resolver = resolver;
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
                // TODO:可能ならファイルから抽出する
                ModuleType[] items = {
                    new ModuleType("buildmodule",         _Resolver.Resolve("{20104,  69901}")),
                    new ModuleType("connectionmodule",    _Resolver.Resolve("{20104,  59901}")),
                    new ModuleType("defencemodule",       _Resolver.Resolve("{20104,  49901}")),
                    new ModuleType("dockarea",            _Resolver.Resolve("{20104,  70001}")),
                    new ModuleType("habitation",          _Resolver.Resolve("{20104,  39901}")),
                    new ModuleType("pier",                _Resolver.Resolve("{20104,  71101}")),
                    new ModuleType("production",          _Resolver.Resolve("{20104,  19901}")),
                    new ModuleType("storage",             _Resolver.Resolve("{20104,  29901}")),
                    new ModuleType("ventureplatform",     _Resolver.Resolve("{20104, 101901}")),
                };

                connection.Execute("INSERT INTO ModuleType(ModuleTypeID, Name) VALUES (@ModuleTypeID, @Name)", items);
            }
        }
    }
}

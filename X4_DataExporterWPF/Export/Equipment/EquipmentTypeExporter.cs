using System.Data;
using Dapper;
using LibX4.Lang;
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
        private readonly LangageResolver _Resolver;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="resolver">言語解決用オブジェクト</param>
        public EquipmentTypeExporter(LangageResolver resolver)
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
                // TODO: 可能ならファイルから抽出する
                EquipmentType[] items =
                {
                    new EquipmentType("countermeasures", _Resolver.Resolve("{20215, 1701}")),
                    new EquipmentType("drones",          _Resolver.Resolve("{20215, 1601}")),
                    new EquipmentType("engines",         _Resolver.Resolve("{20215, 1801}")),
                    new EquipmentType("missiles",        _Resolver.Resolve("{20215, 1901}")),
                    new EquipmentType("shields",         _Resolver.Resolve("{20215, 2001}")),
                    new EquipmentType("software",        _Resolver.Resolve("{20215, 2101}")),
                    new EquipmentType("thrusters",       _Resolver.Resolve("{20215, 2201}")),
                    new EquipmentType("turrets",         _Resolver.Resolve("{20215, 2301}")),
                    new EquipmentType("weapons",         _Resolver.Resolve("{20215, 2401}"))
                };

                connection.Execute("INSERT INTO EquipmentType (EquipmentTypeID, Name) VALUES (@EquipmentTypeID, @Name)", items);
            }
        }
    }
}

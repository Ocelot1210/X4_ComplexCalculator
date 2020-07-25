using System.Data;
using Dapper;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// ウェア生産時の追加効果抽出用クラス
    /// </summary>
    public class CommonExporter : IExporter
    {
        /// <summary>
        /// 現在のDBフォーマットバージョン
        /// </summary>
        public const int CURRENT_FORMAT_VERSION = 1;


        public void Export(IDbConnection connection)
        {
            //////////////////
            // テーブル作成 //
            //////////////////
            {
                // テーブル作成
                connection.Execute(@"
CREATE TABLE IF NOT EXISTS Common
(
    Item    TEXT    NOT NULL PRIMARY KEY,
    Value   INTEGER
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                connection.Execute($"INSERT INTO Common (Item, Value) VALUES ('FormatVersion', {CURRENT_FORMAT_VERSION})");
            }
        }
    }
}

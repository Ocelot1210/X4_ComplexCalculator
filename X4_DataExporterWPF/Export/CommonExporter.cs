using System.Collections.Generic;
using System.Data;
using Dapper;
using X4_DataExporterWPF.Entity;

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
                var items = GetRecords();

                connection.Execute($"INSERT INTO Common (Item, Value) VALUES (@Item, @Value)", items);
            }
        }


        /// <summary>
        /// Common データを返す
        /// </summary>
        /// <returns>Common データ</returns>
        private IEnumerable<Common> GetRecords()
        {
            // TODO: 可能ならファイルから抽出する
            yield return new Common("FormatVersion", CURRENT_FORMAT_VERSION);
        }
    }
}

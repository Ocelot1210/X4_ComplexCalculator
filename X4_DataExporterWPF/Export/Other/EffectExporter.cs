using System.Data;
using Dapper;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// ウェア生産時の追加効果抽出用クラス
    /// </summary>
    public class EffectExporter : IExporter
    {
        public void Export(IDbConnection connection)
        {
            //////////////////
            // テーブル作成 //
            //////////////////
            {
                // テーブル作成
                connection.Execute(@"
CREATE TABLE IF NOT EXISTS Effect
(
    EffectID    TEXT    NOT NULL PRIMARY KEY,
    Name        TEXT    NOT NULL
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                // TODO: 可能ならファイルから抽出する
                Effect[] items = { new Effect("work", "work") };

                // レコード追加
                connection.Execute("INSERT INTO Effect (EffectID, Name) VALUES (@EffectID, @Name)", items);
            }
        }
    }
}

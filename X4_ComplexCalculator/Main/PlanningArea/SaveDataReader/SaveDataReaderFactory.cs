using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.PlanningArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.PlanningArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.PlanningArea.UI.ResourcesGrid;
using X4_ComplexCalculator.Main.PlanningArea.UI.StorageAssign;

namespace X4_ComplexCalculator.Main.PlanningArea.SaveDataReader
{
    static class SaveDataReaderFactory
    {
        /// <summary>
        /// インスタンス作成
        /// </summary>
        /// <param name="path">読み込み対象ファイルパス</param>
        /// <param name="PlanningArea">作業エリア</param>
        /// <returns></returns>
        public static ISaveDataReader CreateSaveDataReader(string path, IPlanningArea PlanningArea)
        {
            var version = GetVersion(path);
            ISaveDataReader ret;

            switch (version)
            {
                case 1:
                    ret = new SaveDataReader1(PlanningArea);
                    break;

                default:
                    ret = new SaveDataReader0(PlanningArea);
                    break;
            }

            ret.Path = path;

            return ret;
        }



        /// <summary>
        /// 保存ファイルのバージョン取得
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>バージョン</returns>
        private static int GetVersion(string path)
        {
            var ret = 0;
            var tableExists = false;

            {
                using var conn = new DBConnection(path);
                conn.ExecQuery("SELECT count(*) AS Count FROM sqlite_master WHERE type = 'table' AND name = 'Common'", (dr, _) =>
                {
                    tableExists = 0L < (long)dr["Count"];
                });
            }

            if (tableExists)
            {
                using var conn = new DBConnection(path);
                conn.ExecQuery("SELECT Value FROM Common WHERE Item = 'FormatVersion'", (dr, _) =>
                {
                    ret = (int)(long)dr["Value"];
                });
            }

            return ret;
        }
    }
}

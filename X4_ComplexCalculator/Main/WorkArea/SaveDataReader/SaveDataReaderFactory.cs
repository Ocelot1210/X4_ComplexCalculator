using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.ResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.StorageAssign;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReader
{
    static class SaveDataReaderFactory
    {
        /// <summary>
        /// インスタンス作成
        /// </summary>
        /// <param name="path">読み込み対象ファイルパス</param>
        /// <param name="modules">モジュール一覧</param>
        /// <param name="products">製品一覧</param>
        /// <param name="resources">建造リソース一覧</param>
        /// <param name="storageAssignItems">保管庫割当情報</param>
        /// <returns></returns>
        public static ISaveDataReader CreateSaveDataReader(
            string path,
            ObservableRangeCollection<ModulesGridItem> modules,
            ObservableRangeCollection<ProductsGridItem> products,
            ObservableRangeCollection<ResourcesGridItem> resources,
            ObservableRangeCollection<StorageAssignGridItem> storageAssignItems
            )
        {
            var version = GetVersion(path);
            ISaveDataReader ret = null;

            switch (version)
            {
                case 1:
                    ret = new SaveDataReader1(modules, products, resources, storageAssignItems);
                    break;

                default:
                    ret = new SaveDataReader0(modules, products, resources);
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

            using (var conn = new DBConnection(path))
            {
                conn.ExecQuery("SELECT count(*) AS Count FROM sqlite_master WHERE type = 'table' AND name = 'Common'", (dr, _) =>
                {
                    tableExists = 0L < (long)dr["Count"];
                });
            }


            if (tableExists)
            {
                using (var conn = new DBConnection(path))
                {
                    conn.ExecQuery("SELECT Value FROM Common WHERE Item = 'FormatVersion'", (dr, _) =>
                    {
                        ret = (int)(long)dr["Value"];
                    });
                }
            }

            return ret;
        }
    }
}

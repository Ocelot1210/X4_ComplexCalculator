﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X4_ComplexCalculator.DB;

namespace X4_ComplexCalculator.Main.StorageGrid
{
    /// <summary>
    /// ドロップダウンで表示するListViewのアイテム(保管庫用)
    /// </summary>
    class StorageDetailsListItem
    {
        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ModuleID { get; private set; }


        /// <summary>
        /// モジュール名
        /// </summary>
        public string ModuleName { get; private set; }


        /// <summary>
        /// モジュール数
        /// </summary>
        public long ModuleCount { get; private set; }


        /// <summary>
        /// 保管庫容量
        /// </summary>
        public long Capacity { get; private set; }


        /// <summary>
        /// 保管庫容量(合計)
        /// </summary>
        public long TotalCapacity => Capacity * ModuleCount;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="moduleCount">モジュール数</param>
        public StorageDetailsListItem(string moduleID, long moduleCount)
        {
            ModuleID = moduleID;
            ModuleCount = moduleCount;

            var query = $@"
SELECT
	Module.Name,
	ModuleStorage.Amount
	
FROM
	Module,
	ModuleStorage
	
WHERE
	Module.ModuleID = ModuleStorage.ModuleID AND
	Module.ModuleID = '{moduleID}'";

            DBConnection.X4DB.ExecQuery(query, (SQLiteDataReader dr, object[] args) => 
            {
                ModuleName = dr["Name"].ToString();
                Capacity = (long)dr["Amount"];
            });
        }


        /// <summary>
        /// モジュールが増えたことにする
        /// </summary>
        /// <param name="count">増分量</param>
        public void Incriment(long count)
        {
            ModuleCount += count;
        }
    }
}

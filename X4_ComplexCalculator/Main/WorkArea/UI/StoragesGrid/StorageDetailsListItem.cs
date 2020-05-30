﻿using Prism.Mvvm;
using System;
using System.Data.SQLite;
using X4_ComplexCalculator.DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StoragesGrid
{
    /// <summary>
    /// ドロップダウンで表示するListViewのアイテム(保管庫用)
    /// </summary>
    class StorageDetailsListItem : BindableBase 
    {
        #region メンバ
        /// <summary>
        /// モジュール数
        /// </summary>
        private long _ModuleCount;
        #endregion

        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ModuleID { get; }


        /// <summary>
        /// モジュール名
        /// </summary>
        public string ModuleName { get; }


        /// <summary>
        /// モジュール数
        /// </summary>
        public long ModuleCount
        {
            get => _ModuleCount;
            set
            {
                if (SetProperty(ref _ModuleCount, value))
                {
                    RaisePropertyChanged(nameof(TotalCapacity));
                }
            }
        }


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

            string? moduleName = null;
            DBConnection.X4DB.ExecQuery(query, (SQLiteDataReader dr, object[] args) => 
            {
                moduleName = (string)dr["Name"];
                Capacity = (long)dr["Amount"];
            });
            ModuleName = moduleName ?? throw new ArgumentException("Invalid moduleID.", nameof(moduleID));
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

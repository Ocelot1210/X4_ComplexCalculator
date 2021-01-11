using System;
using System.Collections.Generic;
using System.Text;

namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 艦船のロードアウト情報
    /// </summary>
    public class ShipLoadout
    {
        /// <summary>
        /// 艦船ID
        /// </summary>
        public string ShipID { get; }

        /// <summary>
        /// ロードアウトID
        /// </summary>
        public string LoadoutID { get; }


        /// <summary>
        /// マクロ名
        /// </summary>
        public string MacroName { get; }


        /// <summary>
        /// 個数
        /// </summary>
        public int Count { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="shipID">艦船ID</param>
        /// <param name="loadoutID">ロードアウトID</param>
        /// <param name="macroName">マクロ名</param>
        /// <param name="count">個数</param>
        public ShipLoadout(
            string shipID,
            string loadoutID,
            string macroName,
            int count)
        {
            ShipID = shipID;
            LoadoutID = loadoutID;
            MacroName = macroName;
            Count = count;
        }
    }
}

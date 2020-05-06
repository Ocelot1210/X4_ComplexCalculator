using System;
using System.Collections.Generic;
using System.Text;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReader
{
    interface ISaveDataReader
    {
        /// <summary>
        /// 読み込み対象ファイルパス
        /// </summary>
        string Path { set; }

        /// <summary>
        /// 保存したファイル読み込み
        /// </summary>
        /// <returns>成功したか</returns>
        bool Load();
    }
}

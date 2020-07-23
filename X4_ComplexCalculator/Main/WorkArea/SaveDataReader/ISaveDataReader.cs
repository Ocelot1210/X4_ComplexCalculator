using System;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReader
{
    /// <summary>
    /// 保存ファイル読み込み処理用インターフェイス
    /// </summary>
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
        bool Load(IProgress<int> progress);
    }
}

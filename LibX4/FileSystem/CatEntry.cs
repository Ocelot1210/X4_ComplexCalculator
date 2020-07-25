namespace LibX4.FileSystem
{
    /// <summary>
    /// catファイルの1レコード分の情報
    /// </summary>
    class CatEntry
    {
        #region メンバ
        /// <summary>
        /// ファイルの実体があるdatファイルパス
        /// </summary>
        public readonly string DatFilePath;


        /// <summary>
        /// ファイル名
        /// </summary>
        public readonly string FileName;


        /// <summary>
        /// ファイルサイズ
        /// </summary>
        public readonly int FileSize;


        /// <summary>
        /// datファイル上のオフセット
        /// </summary>
        public readonly long Offset;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="datFilePath">ファイルの実体があるdatファイルパス</param>
        /// <param name="filename">ファイル名</param>
        /// <param name="fileSize">ファイルサイズ</param>
        /// <param name="offset">datファイル上のオフセット</param>
        public CatEntry(string datFilePath, string filename, int fileSize, long offset)
        {
            DatFilePath = datFilePath;
            FileName = filename;
            FileSize = fileSize;
            Offset = offset;
        }
    }
}

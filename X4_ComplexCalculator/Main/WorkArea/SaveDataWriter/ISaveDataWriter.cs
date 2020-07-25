namespace X4_ComplexCalculator.Main.WorkArea.SaveDataWriter
{
    /// <summary>
    /// ファイル保存処理用インターフェイス
    /// </summary>
    interface ISaveDataWriter
    {
        /// <summary>
        /// 保存先ファイルパス
        /// </summary>
        string SaveFilePath { get; set; }


        /// <summary>
        /// 上書き保存
        /// </summary>
        /// <param name="WorkArea">作業エリア</param>
        /// <returns>保存されたか</returns>
        bool Save(IWorkArea WorkArea);


        /// <summary>
        /// 名前を付けて保存
        /// </summary>
        /// <param name="WorkArea">作業エリア</param>
        /// <returns>保存されたか</returns>
        bool SaveAs(IWorkArea WorkArea);
    }
}

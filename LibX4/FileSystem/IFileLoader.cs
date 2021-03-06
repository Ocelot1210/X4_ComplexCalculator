﻿using System.IO;

namespace LibX4.FileSystem
{
    interface IFileLoader
    {
        /// <summary>
        /// Mod等のルートディレクトリ
        /// </summary>
        public string RootDir { get; }


        /// <summary>
        /// ファイルを開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>ファイルのMemoryStream、該当ファイルが無かった場合はnull</returns>
        public MemoryStream? OpenFile(string filePath);
    }
}

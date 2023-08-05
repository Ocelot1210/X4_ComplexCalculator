using Prism.Mvvm;

namespace X4_DataExporterWPF.ExportWindow.DependencyResolutionFailedWindows
{
    /// <summary>
    /// 依存関係の解決に失敗した情報を表すクラス
    /// </summary>
    internal class DependencyResolutionFailedInfo : BindableBase
    {
        /// <summary>
        /// Mod名
        /// </summary>
        public string ModName { get; }


        /// <summary>
        /// インデント付き Mod 名
        /// </summary>
        public string ModNameWithIndent { get; }


        /// <summary>
        /// Mod ID
        /// </summary>
        public string ID { get; }


        /// <summary>
        /// Mod の格納先
        /// </summary>
        public string FolderName { get; }


        /// <summary>
        /// 備考
        /// </summary>
        public string Remarks { get; }


        /// <summary>
        /// 新しいインスタンスを作成する
        /// </summary>
        /// <param name="modName">Mod名</param>
        /// <param name="id">Mod ID</param>
        /// <param name="folderName">Mod の格納先</param>
        /// <param name="remarks">備考</param>
        /// <param name="level">依存関係の深さ</param>
        internal DependencyResolutionFailedInfo(string modName, string id, string folderName, string remarks, int level)
        {
            ModNameWithIndent = "".PadRight(level * 4) + modName;
            ModName = modName;
            ID = id;
            FolderName = folderName;
            Remarks = remarks;
        }
    }
}

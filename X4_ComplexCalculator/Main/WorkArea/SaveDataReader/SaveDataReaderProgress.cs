using CommunityToolkit.Mvvm.ComponentModel;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataReader
{
    /// <summary>
    /// 保存ファイル読み込み時の進捗表示用
    /// </summary>
    internal partial class SaveDataReaderProgress : ObservableObject
    {
        /// <summary>
        /// ビジー状態か
        /// </summary>
        [ObservableProperty]
        public partial bool IsBusy { get; set; }


        /// <summary>
        /// ファイル読み込み進捗
        /// </summary>
        [ObservableProperty]
        public partial int Progress { get; set; }


        /// <summary>
        /// 読込中のファイル名
        /// </summary>
        [ObservableProperty]
        public partial string LoadingFileName { get; set; } = "";
    }
}

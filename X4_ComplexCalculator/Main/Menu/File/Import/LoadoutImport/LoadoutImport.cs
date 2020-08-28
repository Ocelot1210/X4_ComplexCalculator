using System.Windows.Input;
using Prism.Mvvm;
using WPFLocalizeExtension.Engine;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main.Menu.File.Import.LoadoutImport
{
    /// <summary>
    /// 装備をインポート
    /// </summary>
    class LoadoutImport : BindableBase, IImport
    {
        /// <summary>
        /// メニュー表示用タイトル
        /// </summary>
        public string Title { get; private set; }


        /// <summary>
        /// Viewより呼ばれるCommand
        /// </summary>
        public ICommand Command { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="command">Viewより呼ばれるCommand</param>
        public LoadoutImport(ICommand command)
        {
            Command = command;
            Title = (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:LoadoutImportTitle", null, null);
            LocalizeDictionary.Instance.PropertyChanged += Instance_PropertyChanged;
        }


        /// <summary>
        /// 選択言語変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocalizeDictionary.Instance.Culture))
            {
                Title = (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:LoadoutImportTitle", null, null);
                RaisePropertyChanged(nameof(Title));
            }
        }


        /// <summary>
        /// インポート処理
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        public bool Import(IWorkArea _) => true;    // 何もしない


        /// <summary>
        /// インポート対象を選択
        /// </summary>
        /// <returns>インポート対象数</returns>
        public int Select()
        {
            SelectLoadoutDialog.ShowImportDialog();
            return 0;
        }
    }
}

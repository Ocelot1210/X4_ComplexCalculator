using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace X4_DataExporterWPF.DataExportWindow
{
    /// <summary>
    /// データ抽出処理用ViewModel
    /// </summary>
    class DataExportViewModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// モデル
        /// </summary>
        private readonly DataExportModel _Model = new DataExportModel();

        /// <summary>
        /// 出力先ファイルパス
        /// </summary>
        private readonly string _OutFilePath;
        #endregion


        #region プロパティ
        /// <summary>
        /// 入力元フォルダパス
        /// </summary>
        public ReactiveProperty<string> InDirPath { get; }


        /// <summary>
        /// 言語一覧
        /// </summary>
        public ReactiveCollection<LangComboboxItem> Langages { get; }


        /// <summary>
        /// 選択された言語
        /// </summary>
        public ReactiveProperty<LangComboboxItem?> SelectedLangage { get; }


        /// <summary>
        /// 進捗最大
        /// </summary>
        public ReactiveProperty<int> MaxSteps { get; }


        /// <summary>
        /// 現在の進捗
        /// </summary>
        public ReactiveProperty<int> CurrentStep { get; }


        /// <summary>
        /// ユーザが操作可能か
        /// </summary>
        public ReactiveProperty<bool> CanOperation { get; }


        /// <summary>
        /// 入力元フォルダ参照
        /// </summary>
        public ReactiveCommand SelectInDirCommand { get; }


        /// <summary>
        /// 抽出実行
        /// </summary>
        public AsyncReactiveCommand ExportCommand { get; }


        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        public ReactiveCommand<Window> CloseCommand { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DataExportViewModel(string inDirPath, string outFilePath)
        {
            InDirPath = new ReactiveProperty<string>(inDirPath);
            _OutFilePath = outFilePath;

            Langages = new ReactiveCollection<LangComboboxItem>();
            SelectedLangage = new ReactiveProperty<LangComboboxItem?>();

            MaxSteps = new ReactiveProperty<int>(1);
            CurrentStep = new ReactiveProperty<int>(0);

            CanOperation = new ReactiveProperty<bool>(true);

            // 操作可能かつ入力項目に不備がない場合に true にする
            var canExport = new[]{
                CanOperation,
                InDirPath.Select(p => !string.IsNullOrEmpty(p)),
                SelectedLangage.Select(l => l != null),
            }.CombineLatestValuesAreAllTrue();

            SelectInDirCommand = new ReactiveCommand(CanOperation).WithSubscribe(SelectInDir);
            ExportCommand = new AsyncReactiveCommand(canExport, CanOperation).WithSubscribe(Export);
            CloseCommand = new ReactiveCommand<Window>(CanOperation).WithSubscribe(Close);

            // 入力元フォルダパスが変更された時、言語一覧を更新する
            InDirPath.Subscribe(path =>
            {
                Langages.ClearOnScheduler();
                Langages.AddRangeOnScheduler(_Model.GetLangages(path));
            });
        }


        /// <summary>
        /// 入力元フォルダを選択
        /// </summary>
        private void SelectInDir()
        {
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            dlg.AllowNonFileSystemItems = false;

            if (Directory.Exists(InDirPath.Value))
            {
                dlg.InitialDirectory = InDirPath.Value;
            }
            else
            {
                dlg.InitialDirectory = Path.GetDirectoryName(InDirPath.Value);
            }

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                InDirPath.Value = dlg.FileName;
            }
        }


        /// <summary>
        /// データ抽出実行
        /// </summary>
        private async Task Export()
        {
            // 言語が未選択なら何もしない
            if (SelectedLangage.Value == null)
            {
                return;
            }

            var progless = new Progress<(int currentStep, int maxSteps)>(s =>
            {
                CurrentStep.Value = s.currentStep;
                MaxSteps.Value = s.maxSteps;
            });
            await Task.Run(() => _Model.Export(
                progless,
                InDirPath.Value,
                _OutFilePath,
                SelectedLangage.Value
            ));
            CurrentStep.Value = 0;
        }


        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        /// <param name="window"></param>
        private void Close(Window window) => window.Close();
    }
}

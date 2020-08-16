using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;

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


        /// <summary>
        /// 現在の入力元フォルダパスから言語一覧を取得できなかった場合 true
        /// </summary>
        private readonly ReactivePropertySlim<bool> _InDirPathHasError;


        /// <summary>
        /// 処理状態管理
        /// </summary>
        private readonly BusyNotifier _BusyNotifier = new BusyNotifier();
        #endregion


        #region プロパティ
        /// <summary>
        /// 入力元フォルダパス
        /// </summary>
        public ReactiveProperty<string> InDirPath { get; }


        /// <summary>
        /// 処理中の場合 true、待機中の場合 false
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> IsBusy { get; }


        /// <summary>
        /// 言語一覧
        /// </summary>
        public ReactiveCollection<LangComboboxItem> Languages { get; }


        /// <summary>
        /// 選択された言語
        /// </summary>
        public ReactivePropertySlim<LangComboboxItem?> SelectedLanguage { get; }


        /// <summary>
        /// 進捗最大
        /// </summary>
        public ReactivePropertySlim<int> MaxSteps { get; }


        /// <summary>
        /// 現在の進捗
        /// </summary>
        public ReactivePropertySlim<int> CurrentStep { get; }


        /// <summary>
        /// ユーザが操作可能か
        /// </summary>
        public ReactivePropertySlim<bool> CanOperation { get; }


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
            IsBusy = _BusyNotifier.ToReadOnlyReactivePropertySlim();

            InDirPath = new ReactiveProperty<string>(inDirPath,
                mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe);
            _InDirPathHasError = new ReactivePropertySlim<bool>(false);
            InDirPath.SetValidateNotifyError(
                _ => _InDirPathHasError.Select(isError => isError ? "Error" : null));
            _OutFilePath = outFilePath;

            Languages = new ReactiveCollection<LangComboboxItem>();
            SelectedLanguage = new ReactivePropertySlim<LangComboboxItem?>();

            MaxSteps = new ReactivePropertySlim<int>(1);
            CurrentStep = new ReactivePropertySlim<int>(0);

            CanOperation = new ReactivePropertySlim<bool>(true);

            // 操作可能かつ入力項目に不備がない場合に true にする
            var canExport = new[]{
                CanOperation,
                InDirPath.Select(p => !string.IsNullOrEmpty(p)),
                SelectedLanguage.Select(l => l != null),
            }.CombineLatestValuesAreAllTrue();

            SelectInDirCommand = new ReactiveCommand(CanOperation).WithSubscribe(SelectInDir);
            ExportCommand = new AsyncReactiveCommand(canExport, CanOperation).WithSubscribe(Export);
            CloseCommand = new ReactiveCommand<Window>(CanOperation).WithSubscribe(Close);

            // 入力元フォルダパスに値が代入された時、言語一覧を更新する
            InDirPath.ObserveOn(ThreadPoolScheduler.Instance).Subscribe(path =>
            {
                using var _ = _BusyNotifier.ProcessStart();
                _InDirPathHasError.Value = false;
                Languages.ClearOnScheduler();

                var (success, languages) = _Model.GetLanguages(path);
                _InDirPathHasError.Value = !success;
                Languages.AddRangeOnScheduler(languages);
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
            using var _ = _BusyNotifier.ProcessStart();

            // 言語が未選択なら何もしない
            if (SelectedLanguage.Value == null)
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
                SelectedLanguage.Value
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

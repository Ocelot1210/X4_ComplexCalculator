using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
        private readonly DataExportModel _Model = new();

        /// <summary>
        /// 出力先ファイルパス
        /// </summary>
        private readonly string _OutFilePath;


        /// <summary>
        /// 現在の入力元フォルダパスから言語一覧を取得できなかった場合 true
        /// </summary>
        private readonly ReactivePropertySlim<bool> _UnableToGetLanguages;


        /// <summary>
        /// 処理状態管理
        /// </summary>
        private readonly BusyNotifier _BusyNotifier = new();
        #endregion


        #region プロパティ
        /// <summary>
        /// 入力元フォルダパス
        /// </summary>
        public ReactiveProperty<string> InDirPath { get; }


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
        public ReadOnlyReactivePropertySlim<bool> CanOperation { get; }


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
        public ReactiveCommand<CancelEventArgs> ClosingCommand { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DataExportViewModel(string inDirPath, string outFilePath)
        {
            InDirPath = new ReactiveProperty<string>(inDirPath,
                mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe);
            _UnableToGetLanguages = new ReactivePropertySlim<bool>(false);
            InDirPath.SetValidateNotifyError(
                _ => _UnableToGetLanguages.Select(isError => isError ? "Error" : null));
            _OutFilePath = outFilePath;

            Languages = new ReactiveCollection<LangComboboxItem>();
            SelectedLanguage = new ReactivePropertySlim<LangComboboxItem?>();

            MaxSteps = new ReactivePropertySlim<int>(1);
            CurrentStep = new ReactivePropertySlim<int>(0);

            CanOperation = _BusyNotifier.Inverse().ToReadOnlyReactivePropertySlim();

            // 操作可能かつ入力項目に不備がない場合に true にする
            var canExport = new[]{
                CanOperation,
                InDirPath.Select(p => !string.IsNullOrEmpty(p)),
                SelectedLanguage.Select(l => l != null),
            }.CombineLatestValuesAreAllTrue();

            SelectInDirCommand = new ReactiveCommand(CanOperation).WithSubscribe(SelectInDir);
            ExportCommand = new AsyncReactiveCommand(canExport).WithSubscribe(Export);
            ClosingCommand = new ReactiveCommand<CancelEventArgs>().WithSubscribe(Closing);

            // 入力元フォルダパスに値が代入された時、言語一覧を更新する
            InDirPath.ObserveOn(ThreadPoolScheduler.Instance).Subscribe(path =>
            {
                using var _ = _BusyNotifier.ProcessStart();
                _UnableToGetLanguages.Value = false;
                Languages.ClearOnScheduler();

                var (success, languages) = _Model.GetLanguages(path);
                _UnableToGetLanguages.Value = !success;
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
        /// <param name="e"></param>
        private void Closing(CancelEventArgs e) => e.Cancel = _BusyNotifier.IsBusy;
    }
}

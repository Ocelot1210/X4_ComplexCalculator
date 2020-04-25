﻿using Prism.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Main.WorkArea;
using Xceed.Wpf.AvalonDock;

namespace X4_ComplexCalculator.Main
{
    /// <summary>
    /// メイン画面のViewModel
    /// </summary>
    class MainWindowViewModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// メイン画面のModel
        /// </summary>
        private readonly MainWindowModel _Model;
        #endregion

        #region プロパティ
        /// <summary>
        /// Windowが閉じられる時
        /// </summary>
        public ICommand WindowClosingCommand { get; }

        /// <summary>
        /// 新規作成
        /// </summary>
        public ICommand CreateNewCommand { get; }


        /// <summary>
        /// 上書き保存
        /// </summary>
        public ICommand SaveCommand { get; }


        /// <summary>
        /// 名前を指定して保存
        /// </summary>
        public ICommand SaveAsCommand { get; }


        /// <summary>
        /// 開く
        /// </summary>
        public ICommand OpenCommand { get; }


        /// <summary>
        /// DB更新
        /// </summary>
        public ICommand UpdateDBCommand { get; }


        /// <summary>
        /// タブが閉じられる時
        /// </summary>
        public ICommand DocumentClosingCommand { get; }


        /// <summary>
        /// ワークエリア一覧
        /// </summary>
        public ObservableCollection<WorkAreaViewModel> Documents => _Model.Documents;


        /// <summary>
        /// アクティブなワークスペース
        /// </summary>
        public WorkAreaViewModel ActiveContent
        {
            set
            {
                _Model.ActiveContent = value;
            }
            get
            {
                return _Model.ActiveContent;
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            _Model                 = new MainWindowModel();
            WindowClosingCommand   = new DelegateCommand<CancelEventArgs>(WindowClosing);
            CreateNewCommand       = new DelegateCommand(CreateNew);
            SaveCommand            = new DelegateCommand(_Model.Save);
            SaveAsCommand          = new DelegateCommand(_Model.SaveAs);
            OpenCommand            = new DelegateCommand(Open);
            UpdateDBCommand        = new DelegateCommand(_Model.UpdateDB);
            DocumentClosingCommand = new DelegateCommand<DocumentClosingEventArgs>(DocumentClosing);
        }


        /// <summary>
        /// 新規作成
        /// </summary>
        private void CreateNew()
        {
            _Model.CreateNew();
            OnPropertyChanged(nameof(ActiveContent));
        }


        /// <summary>
        /// 開く
        /// </summary>
        private void Open()
        {
            _Model.Open();
            OnPropertyChanged(nameof(ActiveContent));
        }


        /// <summary>
        /// ウィンドウが閉じられる時
        /// </summary>
        private void WindowClosing(CancelEventArgs e)
        {
            // 未保存の内容が存在するか？
            if (Documents.Where(x => x.HasChanged).Any())
            {
                var result = MessageBox.Show("未保存の項目があります。保存しますか？", "確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (result)
                {
                    // 保存する場合
                    case MessageBoxResult.Yes:
                        foreach (var doc in Documents)
                        {
                            doc.Save();
                        }
                        break;

                    // 保存せずに閉じる場合
                    case MessageBoxResult.No:
                        break;

                    // キャンセルする場合
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        /// <summary>
        /// タブが閉じられる時
        /// </summary>
        /// <param name="e"></param>
        private void DocumentClosing(DocumentClosingEventArgs e)
        {
            if (e.Document.Content is WorkAreaViewModel workArea)
            {
                // 変更があったか？
                if (workArea.HasChanged)
                {
                    var result = MessageBox.Show("変更内容を保存しますか？", "確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                    switch (result)
                    {
                        // 保存する場合
                        case MessageBoxResult.Yes:
                            workArea.Save();
                            break;

                        // 保存せずに閉じる場合
                        case MessageBoxResult.No:
                            break;

                        // キャンセルする場合
                        case MessageBoxResult.Cancel:
                            e.Cancel = true;
                            break;
                    }
                }

                if (!e.Cancel)
                {
                    workArea.Dispose();
                }
            }
        }
    }
}

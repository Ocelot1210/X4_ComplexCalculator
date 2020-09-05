﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using Reactive.Bindings.Extensions;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Dialog.SelectStringDialog;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.Menu.Layout;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main
{
    /// <summary>
    /// レイアウト管理用クラス
    /// </summary>
    class LayoutsManager : IDisposable
    {
        #region メンバ
        /// <summary>
        /// 現在のレイアウト
        /// </summary>
        private LayoutMenuItem? _ActiveLayout;


        /// <summary>
        /// 作業エリア管理用
        /// </summary>
        private readonly WorkAreaManager _WorkAreaManager;


        /// <summary>
        /// Dispose が必要なオブジェクトのコレクション
        /// </summary>
        private readonly CompositeDisposable _Disposables = new CompositeDisposable();
        #endregion


        #region プロパティ
        /// <summary>
        /// レイアウト一覧
        /// </summary>
        public ObservablePropertyChangedCollection<LayoutMenuItem> Layouts { get; } = new ObservablePropertyChangedCollection<LayoutMenuItem>();


        /// <summary>
        /// 現在のレイアウト
        /// </summary>
        public LayoutMenuItem? ActiveLayout
        {
            get => _ActiveLayout;
            private set
            {
                if (_ActiveLayout != value)
                {
                    _ActiveLayout = value;
                    if (_ActiveLayout != null)
                    {
                        foreach (var document in _WorkAreaManager.Documents)
                        {
                            document.SetLayout(_ActiveLayout.LayoutID);
                        }
                    }
                }
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="workAreaManager">作業エリア管理用オブジェクト</param>
        public LayoutsManager(WorkAreaManager workAreaManager)
        {
            _WorkAreaManager = workAreaManager;
            Layouts.CollectionPropertyChanged += Layouts_CollectionPropertyChanged;

            // レイアウト一覧の上書きボタンをトリガーにレイアウト上書き保存を実行
            Layouts.ObserveElementObservableProperty(x => x.SaveButtonClickedCommand)
                .Select(x => x.Instance)
                .Subscribe(OverwritedSaveLayout)
                .AddTo(_Disposables);
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            // レイアウト一覧読み込み
            SettingDatabase.Instance.ExecQuery("SELECT LayoutID, LayoutName, IsChecked FROM WorkAreaLayouts", (dr, args) =>
            {
                Layouts.Add(new LayoutMenuItem((long)dr["LayoutID"], (string)dr["LayoutName"], (long)dr["IsChecked"] == 1));
            });

            var checkedLayout = Layouts.FirstOrDefault(x => x.IsChecked);
            if (checkedLayout != null)
            {
                ActiveLayout = checkedLayout;
            }
        }


        /// <summary>
        /// レイアウト保存
        /// </summary>
        public void SaveLayout(WorkAreaViewModel? vm)
        {
            if (vm != null)
            {
                var (onOK, layoutName) = SelectStringDialog.ShowDialog("Lang:EditLayoutName", "Lang:LayoutName", "", LayoutMenuItem.IsValidLayoutName);
                if (onOK)
                {
                    try
                    {
                        SettingDatabase.Instance.BeginTransaction();
                        var layoutID = vm.SaveLayout(layoutName);
                        SettingDatabase.Instance.Commit();

                        Layouts.Add(new LayoutMenuItem(layoutID, layoutName, false));
                    }
                    catch (Exception ex)
                    {
                        SettingDatabase.Instance.Rollback();
                        LocalizedMessageBox.Show("Lang:LayoutSaveFailedMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, ex.Message);
                    }

                    LocalizedMessageBox.Show("Lang:LayoutSavedMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, vm.Title, layoutName);
                }
            }
            else
            {
                LocalizedMessageBox.Show("Lang:TabDoesNotSelectedMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        /// <summary>
        /// レイアウト上書き保存
        /// </summary>
        /// <param name="menuItem">上書きするレイアウト</param>
        public void OverwritedSaveLayout(LayoutMenuItem menuItem)
        {
            if (_WorkAreaManager.ActiveContent == null) return;

            try
            {
                SettingDatabase.Instance.BeginTransaction();
                _WorkAreaManager.ActiveContent.OverwriteSaveLayout(menuItem.LayoutID);
                SettingDatabase.Instance.Commit();

                LocalizedMessageBox.Show("Lang:LayoutOverwritedMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, _WorkAreaManager.ActiveContent.Title, menuItem.LayoutName);
            }
            catch (Exception ex)
            {
                SettingDatabase.Instance.Rollback();
                LocalizedMessageBox.Show("Lang:LayoutOverwriteFailedMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, ex.Message);
            }
        }


        /// <summary>
        /// レイアウト一覧のプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Layouts_CollectionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!(sender is LayoutMenuItem menuItem))
            {
                return;
            }


            switch (e.PropertyName)
            {
                // チェック状態
                case nameof(LayoutMenuItem.IsChecked):
                    if (menuItem.IsChecked)
                    {
                        // プリセットが選択された場合、他のチェックを全部外す
                        foreach (var layout in Layouts.Where(x => x != menuItem))
                        {
                            layout.IsChecked = false;
                        }

                        ActiveLayout = menuItem;
                    }
                    else
                    {
                        ActiveLayout = null;
                    }
                    break;

                // 削除されたか
                case nameof(LayoutMenuItem.IsDeleted):
                    if (menuItem.IsDeleted)
                    {
                        Layouts.Remove(menuItem);
                        ActiveLayout = null;
                    }
                    break;

                default:
                    break;
            }
        }


        /// <inheritdoc />
        public void Dispose() => _Disposables.Dispose();
    }
}

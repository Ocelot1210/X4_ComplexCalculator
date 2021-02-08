using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
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
        private readonly ReactivePropertySlim<LayoutMenuItem?> _ActiveLayout = new();


        /// <summary>
        /// 作業エリア管理用
        /// </summary>
        private readonly WorkAreaManager _WorkAreaManager;


        /// <summary>
        /// Dispose が必要なオブジェクトのコレクション
        /// </summary>
        private readonly CompositeDisposable _Disposables = new();
        #endregion


        #region プロパティ
        /// <summary>
        /// レイアウト一覧
        /// </summary>
        public ObservableCollection<LayoutMenuItem> Layouts { get; } = new();


        /// <summary>
        /// 現在のレイアウト
        /// </summary>
        public IReadOnlyReactiveProperty<LayoutMenuItem?> ActiveLayout => _ActiveLayout;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="workAreaManager">作業エリア管理用オブジェクト</param>
        public LayoutsManager(WorkAreaManager workAreaManager)
        {
            _WorkAreaManager = workAreaManager;

            // レイアウト一覧の上書きボタンをトリガーにレイアウト上書き保存を実行
            Layouts.ObserveElementObservableProperty(x => x.SaveButtonClickedCommand)
                .Select(x => x.Instance)
                .Subscribe(OverwritedSaveLayout)
                .AddTo(_Disposables);

            // レイアウト一覧の変更ボタンをトリガーにレイアウト名変更を実行
            Layouts.ObserveElementObservableProperty(x => x.EditButtonClickedCommand)
                .Select(x => x.Instance)
                .Subscribe(EditLayoutName)
                .AddTo(_Disposables);

            // レイアウト一覧の削除ボタンをトリガーにレイアウト削除を実行
            Layouts.ObserveElementObservableProperty(x => x.DeleteButtonClickedCommand)
                .Select(x => x.Instance)
                .Subscribe(DeleteLayout)
                .AddTo(_Disposables);

            // プリセットが選択、または解除された場合、DB に状態を保存する
            var changeChecked = Layouts.ObserveElementObservableProperty(x => x.IsChecked);
            changeChecked.Select(x => x.Instance)
                .Subscribe(x => SettingDatabase.Instance.Execute($"UPDATE WorkAreaLayouts SET IsChecked = :IsChecked WHERE LayoutID = :LayoutID", new { IsChecked = x.IsChecked.Value ? 1 : 0, x.LayoutID }));

            // プリセットが選択された場合、他のチェックを全部外す
            changeChecked.Where(x => x.Value)
                .Select(x => x.Instance)
                .Subscribe(ExclusiveChecked);
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            // レイアウト一覧読み込み
            const string sql = "SELECT LayoutID, LayoutName, IsChecked FROM WorkAreaLayouts";
            var items = SettingDatabase.Instance.Query<(long LayoutID, string LayoutName, bool IsChecked)>(sql);
            foreach (var (layoutID, layoutName, isChecked) in items)
            {
                Layouts.Add(new LayoutMenuItem(layoutID, layoutName, isChecked));
            }

            var checkedLayout = Layouts.FirstOrDefault(x => x.IsChecked.Value);
            if (checkedLayout is not null)
            {
                _ActiveLayout.Value = checkedLayout;
            }
        }


        /// <summary>
        /// レイアウト保存
        /// </summary>
        public void SaveLayout(WorkAreaViewModel? vm)
        {
            if (vm is not null)
            {
                var (onOK, layoutName) = SelectStringDialog.ShowDialog("Lang:EditLayoutName", "Lang:LayoutName", "", IsValidLayoutName);
                if (onOK)
                {
                    try
                    {
                        var layoutID = vm.SaveLayout(layoutName);

                        Layouts.Add(new LayoutMenuItem(layoutID, layoutName, false));
                    }
                    catch (Exception ex)
                    {
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
        private void OverwritedSaveLayout(LayoutMenuItem menuItem)
        {
            if (_WorkAreaManager.ActiveContent is null) return;

            try
            {
                _WorkAreaManager.ActiveContent.OverwriteSaveLayout(menuItem.LayoutID);

                LocalizedMessageBox.Show("Lang:LayoutOverwritedMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, _WorkAreaManager.ActiveContent.Title, menuItem.LayoutName);
            }
            catch (Exception ex)
            {
                LocalizedMessageBox.Show("Lang:LayoutOverwriteFailedMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, ex.Message);
            }
        }


        /// <summary>
        /// レイアウト名変更
        /// </summary>
        private void EditLayoutName(LayoutMenuItem menuItem)
        {
            var (onOK, newLayoutName) = SelectStringDialog.ShowDialog("Lang:EditLayoutName", "Lang:LayoutName", menuItem.LayoutName.Value, IsValidLayoutName);
            if (onOK && menuItem.LayoutName.Value != newLayoutName)
            {
                menuItem.LayoutName.Value = newLayoutName;

                const string sql = "UPDATE WorkAreaLayouts SET LayoutName = :LayoutName WHERE LayoutID = :LayoutID";

                SettingDatabase.Instance.Execute(sql, new { LayoutName = menuItem.LayoutName.Value, LayoutID = menuItem.LayoutID });
            }
        }


        /// <summary>
        /// レイアウト削除
        /// </summary>
        private void DeleteLayout(LayoutMenuItem menuItem)
        {
            var result = LocalizedMessageBox.Show("Lang:DeleteLayoutConfirmMessage", "Lang:Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No, menuItem.LayoutName);

            if (result == MessageBoxResult.Yes)
            {
                SettingDatabase.Instance.Execute("DELETE FROM WorkAreaLayouts WHERE LayoutID = :LayoutID", new { menuItem.LayoutID });
                Layouts.Remove(menuItem);
                _ActiveLayout.Value = null;
            }
        }


        /// <summary>
        /// レイアウト名が有効か判定
        /// </summary>
        /// <param name="layoutName">レイアウト名</param>
        /// <returns>レイアウト名が有効か</returns>
        private static bool IsValidLayoutName(string layoutName)
        {
            var ret = true;

            if (string.IsNullOrWhiteSpace(layoutName))
            {
                LocalizedMessageBox.Show("Lang:InvalidLayoutNameMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                ret = false;
            }

            return ret;
        }


        /// <summary>
        /// プリセットが選択された場合、他のチェックを全部外す
        /// </summary>
        /// <param name="menuItem">選択されたプリセット</param>
        private void ExclusiveChecked(LayoutMenuItem menuItem)
        {
            if (menuItem.IsChecked.Value)
            {
                foreach (var layout in Layouts.Where(x => x != menuItem))
                {
                    layout.IsChecked.Value = false;
                }

                _ActiveLayout.Value = menuItem;
            }
        }


        /// <inheritdoc />
        public void Dispose() => _Disposables.Dispose();
    }
}

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
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
        private readonly ReactivePropertySlim<LayoutMenuItem?> _ActiveLayout
            = new ReactivePropertySlim<LayoutMenuItem?>();


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
        public ObservableCollection<LayoutMenuItem> Layouts { get; }
            = new ObservableCollection<LayoutMenuItem>();


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
                .Subscribe(x => SettingDatabase.Instance.ExecQuery($"UPDATE WorkAreaLayouts SET IsChecked = {(x.IsChecked.Value ? 1 : 0)} WHERE LayoutID = {x.LayoutID}"));

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
            SettingDatabase.Instance.ExecQuery("SELECT LayoutID, LayoutName, IsChecked FROM WorkAreaLayouts", (dr, _) =>
            {
                Layouts.Add(new LayoutMenuItem((long)dr["LayoutID"], (string)dr["LayoutName"], (bool)dr["IsChecked"]));
            });

            var checkedLayout = Layouts.FirstOrDefault(x => x.IsChecked.Value);
            if (checkedLayout != null)
            {
                _ActiveLayout.Value = checkedLayout;
            }
        }


        /// <summary>
        /// レイアウト保存
        /// </summary>
        public void SaveLayout(WorkAreaViewModel? vm)
        {
            if (vm != null)
            {
                var (onOK, layoutName) = SelectStringDialog.ShowDialog("Lang:EditLayoutName", "Lang:LayoutName", "", IsValidLayoutName);
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
        /// レイアウト名変更
        /// </summary>
        private void EditLayoutName(LayoutMenuItem menuItem)
        {
            var (onOK, newLayoutName) = SelectStringDialog.ShowDialog("Lang:EditLayoutName", "Lang:LayoutName", menuItem.LayoutName.Value, IsValidLayoutName);
            if (onOK && menuItem.LayoutName.Value != newLayoutName)
            {
                menuItem.LayoutName.Value = newLayoutName;

                var param = new SQLiteCommandParameters(2);
                param.Add("layoutName", System.Data.DbType.String, menuItem.LayoutName.Value);
                param.Add("layoutID", System.Data.DbType.Int32, menuItem.LayoutID);
                SettingDatabase.Instance.ExecQuery($"UPDATE WorkAreaLayouts SET LayoutName = :layoutName WHERE LayoutID = :layoutID", param);
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
                SettingDatabase.Instance.ExecQuery($"DELETE FROM WorkAreaLayouts WHERE LayoutID = {menuItem.LayoutID}");
                Layouts.Remove(menuItem);
                _ActiveLayout.Value = null;
            }
        }


        /// <summary>
        /// レイアウト名が有効か判定
        /// </summary>
        /// <param name="layoutName">レイアウト名</param>
        /// <returns>レイアウト名が有効か</returns>
        public static bool IsValidLayoutName(string layoutName)
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

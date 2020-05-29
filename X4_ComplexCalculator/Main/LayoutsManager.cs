using System.Collections.Generic;
using System.Linq;
using System.Windows;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.Menu.Layout;

namespace X4_ComplexCalculator.Main
{
    /// <summary>
    /// レイアウト管理用クラス
    /// </summary>
    class LayoutsManager
    {
        #region メンバ
        /// <summary>
        /// 現在のレイアウト
        /// </summary>
        private LayoutMenuItem _ActiveLayout;


        /// <summary>
        /// 作業エリア管理用
        /// </summary>
        private readonly WorkAreaManager _WorkAreaManager;
        #endregion


        #region プロパティ
        /// <summary>
        /// レイアウト一覧
        /// </summary>
        public ObservablePropertyChangedCollection<LayoutMenuItem> Layouts = new ObservablePropertyChangedCollection<LayoutMenuItem>();


        /// <summary>
        /// 現在のレイアウト
        /// </summary>
        public LayoutMenuItem ActiveLayout
        {
            get => _ActiveLayout;
            set
            {
                if (ActiveLayout != value)
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
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            // レイアウト一覧読み込み
            var layouts = new List<LayoutMenuItem>();
            DBConnection.CommonDB.ExecQuery("SELECT LayoutID, LayoutName, IsChecked FROM WorkAreaLayouts", (dr, args) =>
            {
                layouts.Add(new LayoutMenuItem((long)dr["LayoutID"], (string)dr["LayoutName"], (long)dr["IsChecked"] == 1));
            });

            var checkedLayout = layouts.Where(x => x.IsChecked).FirstOrDefault();
            if (checkedLayout != null)
            {
                ActiveLayout = checkedLayout;
            }

            Layouts.AddRange(layouts);
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

                // レイアウト上書き保存
                case nameof(LayoutMenuItem.SaveButtonClickedCommand):
                    if (_WorkAreaManager.ActiveContent != null)
                    {
                        _WorkAreaManager.ActiveContent.OverwriteSaveLayout(menuItem.LayoutID);

                        Localize.ShowMessageBox("Lang:LayoutOverwritedMessage", "Lang:Confirmation", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, _WorkAreaManager.ActiveContent.Title, menuItem.LayoutName);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}

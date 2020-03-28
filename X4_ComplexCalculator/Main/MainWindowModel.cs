using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Threading;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.ModulesGrid;
using X4_ComplexCalculator.Main.ProductsGrid;
using X4_ComplexCalculator.Main.ResourcesGrid;

namespace X4_ComplexCalculator.Main
{
    class MainWindowModel
    {
        #region メンバ
        /// <summary>
        /// 保存したファイルのパス
        /// </summary>
        private string _SaveFilePath = "";

        /// <summary>
        /// モジュール一覧
        /// </summary>
        private readonly SmartCollection<ModulesGridItem> _Modules;


        /// <summary>
        /// 製品一覧
        /// </summary>
        private readonly SmartCollection<ProductsGridItem> _Products;


        /// <summary>
        /// 建造リソース一覧
        /// </summary>
        private readonly SmartCollection<ResourcesGridItem> _Resources;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modules">モジュール一覧</param>
        /// <param name="products">製品一覧</param>
        /// <param name="resources">建造リソース一覧</param>
        public MainWindowModel(SmartCollection<ModulesGridItem> modules, SmartCollection<ProductsGridItem> products, SmartCollection<ResourcesGridItem> resources)
        {
            _Modules  = modules;
            _Products = products;
            _Resources = resources;
        }

        /// <summary>
        /// 上書き保存
        /// </summary>
        public void Save()
        {
            if (_SaveFilePath == "")
            {
                SaveAs();
                return;
            }

            SaveMain();
        }


        /// <summary>
        /// 名前を指定して保存
        /// </summary>
        public void SaveAs()
        {
            var dlg = new SaveFileDialog();

            dlg.Filter = "X4 Station calclator data (*.x4)|*.x4|All Files|*.*";
            if (dlg.ShowDialog() == true)
            {
                _SaveFilePath = dlg.FileName;
                SaveMain();
            }
        }

        /// <summary>
        /// 保存処理メイン
        /// </summary>
        private void SaveMain()
        {
            using var conn = new DBConnection(_SaveFilePath);
            conn.BeginTransaction();

            // 保存用テーブル初期化
            conn.ExecQuery("DROP TABLE IF EXISTS Modules");
            conn.ExecQuery("DROP TABLE IF EXISTS Equipments");
            conn.ExecQuery("DROP TABLE IF EXISTS Products");
            conn.ExecQuery("DROP TABLE IF EXISTS BuildResources");
            conn.ExecQuery("CREATE TABLE Modules(Row INTEGER, ModuleID TEXT, Count INTEGER)");
            conn.ExecQuery("CREATE TABLE Equipments(Row INTEGER, EquipmentID TEXT)");
            conn.ExecQuery("CREATE TABLE Products(WareID TEXT, Price INTEGER)");
            conn.ExecQuery("CREATE TABLE BuildResources(WareID TEXT, Price INTEGER)");

            // モジュール保存
            var rowCnt = 0;
            foreach (var module in _Modules)
            {
                conn.ExecQuery($"INSERT INTO Modules(Row, ModuleID, Count) Values({rowCnt}, '{module.Module.ModuleID}', {module.ModuleCount})");

                foreach (var equipment in module.Module.Equipment.GetAllEquipment())
                {
                    conn.ExecQuery($"INSERT INTO Equipments(Row, EquipmentID) Values({rowCnt}, '{equipment.EquipmentID}')");
                }

                rowCnt++;
            }

            // 価格保存
            foreach (var product in _Products)
            {
                conn.ExecQuery($"INSERT INTO Products(WareID, Price) Values('{product.Ware.WareID}', {product.UnitPrice})");
            }

            // 建造リソース保存
            foreach(var resource in _Resources)
            {
                conn.ExecQuery($"INSERT INTO BuildResources(WareID, Price) Values('{resource.Ware.WareID}', {resource.UnitPrice})");
            }

            conn.Commit();
        }


        /// <summary>
        /// 開く
        /// </summary>
        public void Open()
        {
            var dlg = new OpenFileDialog();

            dlg.Filter = "X4 Station calclator data (*.x4)|*.x4|All Files|*.*";
            if (dlg.ShowDialog() == true)
            {
                _SaveFilePath = dlg.FileName;

                using (Dispatcher.CurrentDispatcher.DisableProcessing())
                {
                    OpenMain();
                }
            }
        }

        /// <summary>
        /// 開くメイン処理
        /// </summary>
        private void OpenMain()
        {
            try
            {
                using var conn = new DBConnection(_SaveFilePath);

                // モジュール復元
                RestoreModules(conn);

                // 製品価格を復元
                RestoreProductsPrice(conn);

                // 建造リソースを復元
                RestoreBuildResource(conn);
            }
            catch(Exception e)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(() => 
                {
                    MessageBox.Show($"ファイルの読み込みに失敗しました。\r\n\r\n■理由：\r\n{e.Message}", "読み込み失敗", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        /// <summary>
        /// モジュールを復元
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="args"></param>
        private void RestoreModules(DBConnection conn)
        {
            var modules = new List<ModulesGridItem>();

            // モジュールを復元
            conn.ExecQuery("SELECT * FROM Modules", (SQLiteDataReader modDr, object[] _) =>
            {
                var module = new ModulesGridItem(modDr["ModuleID"].ToString(), (long)modDr["Count"]);

                // モジュールの装備を復元
                conn.ExecQuery($"SELECT EquipmentID FROM Equipments WHERE Row = {(long)modDr["Row"]}", (SQLiteDataReader eqDr, object[] _) =>
                {
                    var eqipment = new Equipment(eqDr["EquipmentID"].ToString());

                    module.Module.AddEquipment(eqipment);
                });

                modules.Add(module);
            });

            _Modules.Reset(modules);
        }


        /// <summary>
        /// 製品価格を復元
        /// </summary>
        /// <param name="conn"></param>
        private void RestoreProductsPrice(DBConnection conn)
        {
            foreach(var product in _Products)
            {
                conn.ExecQuery($"SELECT Price FROM Products WHERE WareID = '{product.Ware.WareID}'", (SQLiteDataReader dr, object[] _) =>
                {
                    product.UnitPrice = (long)dr["Price"];
                });
            }
        }


        /// <summary>
        /// 建造リソースを復元
        /// </summary>
        /// <param name="conn"></param>
        private void RestoreBuildResource(DBConnection conn)
        {
            foreach (var resource in _Resources)
            {
                conn.ExecQuery($"SELECT Price FROM BuildResources WHERE WareID = '{resource.Ware.WareID}'", (SQLiteDataReader dr, object[] _) =>
                {
                    resource.UnitPrice = (long)dr["Price"];
                });
            }
        }

        /// <summary>
        /// DB更新
        /// </summary>
        public void UpdateDB()
        {
            var result = MessageBox.Show("DB更新画面を表示しますか？\r\n※ 画面が起動するまでしばらくお待ち下さい。", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (!DBConnection.UpdateDB())
                {
                    MessageBox.Show("DBの更新に失敗しました。\r\nDBファイルにアクセス可能か確認後、再度実行してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

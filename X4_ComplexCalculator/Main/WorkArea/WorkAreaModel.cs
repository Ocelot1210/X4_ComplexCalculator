using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.SQLite;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.ModulesGrid;
using X4_ComplexCalculator.Main.ProductsGrid;
using X4_ComplexCalculator.Main.ResourcesGrid;

namespace X4_ComplexCalculator.Main.WorkArea
{
    /// <summary>
    /// 作業エリア用Model
    /// </summary>
    class WorkAreaModel : INotifyPropertyChangedBace, IDisposable
    {
        #region メンバ
        /// <summary>
        /// モジュール一覧
        /// </summary>
        private readonly ModulesGridModel _Modules;


        /// <summary>
        /// 製品一覧
        /// </summary>
        private readonly ProductsGridModel _Products;


        /// <summary>
        /// 建造リソース一覧
        /// </summary>
        private readonly ResourcesGridModel _Resources;


        /// <summary>
        /// 変更されたか
        /// </summary>
        private bool _HasChanged;
        #endregion


        #region プロパティ
        /// <summary>
        /// 保存先ファイルパス
        /// </summary>
        public string SaveFilePath { get; private set; }


        /// <summary>
        /// 変更されたか
        /// </summary>
        public bool HasChanged
        {
            get
            {
                return _HasChanged;
            }
            set
            {
                if (value != _HasChanged)
                {
                    _HasChanged = value;
                    OnPropertyChanged();

                    if (value)
                    {
                        // 変更検知イベントを購読解除
                        _Modules.Modules.CollectionChanged -= OnModulesChanged;
                        _Modules.Modules.OnCollectionPropertyChanged -= OnPropertyChanged;
                        _Products.Products.OnCollectionPropertyChanged -= OnPropertyChanged;
                        _Resources.Resources.OnCollectionPropertyChanged -= OnPropertyChanged;
                    }
                    else
                    {
                        // 変更検知イベントを購読
                        _Modules.Modules.CollectionChanged += OnModulesChanged;
                        _Modules.Modules.OnCollectionPropertyChanged += OnPropertyChanged;
                        _Products.Products.OnCollectionPropertyChanged += OnPropertyChanged;
                        _Resources.Resources.OnCollectionPropertyChanged += OnPropertyChanged;
                    }
                }
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modules">モジュール一覧</param>
        /// <param name="products">製品一覧</param>
        /// <param name="resources">建造リソース一覧</param>
        public WorkAreaModel(ModulesGridModel modules, ProductsGridModel products, ResourcesGridModel resources)
        {
            _Modules = modules;
            _Products = products;
            _Resources = resources;

            HasChanged = true;
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            // 変更検知イベントを購読解除
            _Modules.Modules.CollectionChanged -= OnModulesChanged;
            _Modules.Modules.OnCollectionPropertyChanged -= OnPropertyChanged;
            _Products.Products.OnCollectionPropertyChanged -= OnPropertyChanged;
            _Resources.Resources.OnCollectionPropertyChanged -= OnPropertyChanged;
        }


        /// <summary>
        /// モジュール数に変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModulesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HasChanged = true;
        }

        /// <summary>
        /// コレクションのプロパティに変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string[] names =
            {
                nameof(ModulesGridItem.ModuleCount),
                nameof(ProductsGridItem.Price),
                nameof(ResourcesGridItem.Price),
            };

            if (0 < Array.IndexOf(names, e.PropertyName))
            {
                HasChanged = true;
            }
        }


        /// <summary>
        /// 上書き保存
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(SaveFilePath))
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
                SaveFilePath = dlg.FileName;
                SaveMain();
            }
        }

        /// <summary>
        /// 保存処理メイン
        /// </summary>
        private void SaveMain()
        {
            using var conn = new DBConnection(SaveFilePath);
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
            foreach (var module in _Modules.Modules)
            {
                conn.ExecQuery($"INSERT INTO Modules(Row, ModuleID, Count) Values({rowCnt}, '{module.Module.ModuleID}', {module.ModuleCount})");

                foreach (var equipment in module.Module.Equipment.GetAllEquipment())
                {
                    conn.ExecQuery($"INSERT INTO Equipments(Row, EquipmentID) Values({rowCnt}, '{equipment.EquipmentID}')");
                }

                rowCnt++;
            }

            // 価格保存
            foreach (var product in _Products.Products)
            {
                conn.ExecQuery($"INSERT INTO Products(WareID, Price) Values('{product.Ware.WareID}', {product.UnitPrice})");
            }

            // 建造リソース保存
            foreach (var resource in _Resources.Resources)
            {
                conn.ExecQuery($"INSERT INTO BuildResources(WareID, Price) Values('{resource.Ware.WareID}', {resource.UnitPrice})");
            }

            conn.Commit();
            HasChanged = false;
        }



        /// <summary>
        /// ファイル読み込み
        /// </summary>
        /// <param name="path"></param>
        public void Load(string path)
        {
            using var conn = new DBConnection(path);
            conn.BeginTransaction();

            // モジュール復元
            RestoreModules(conn);

            // 製品価格を復元
            RestoreProductsPrice(conn);

            // 建造リソースを復元
            RestoreBuildResource(conn);

            conn.Rollback();

            SaveFilePath = path;

            HasChanged = false;
        }


        /// <summary>
        /// モジュールを復元
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="args"></param>
        private void RestoreModules(DBConnection conn)
        {
            var moduleCnt = 0;

            conn.ExecQuery("SELECT count(*) AS Count from Modules", (dr, _) =>
            {
                moduleCnt = (int)(long)dr[0];
            });

            var modules = new List<ModulesGridItem>(moduleCnt);

            // モジュールを復元
            conn.ExecQuery("SELECT ModuleID, Count FROM Modules ORDER BY Row ASC", (dr, _) =>
            {
                modules.Add(new ModulesGridItem(dr["ModuleID"].ToString(), (long)dr["Count"]));
            });

            // モジュールの装備を復元
            conn.ExecQuery($"SELECT * FROM Equipments", (dr, _) =>
            {
                modules[(int)(long)dr["row"]].Module.AddEquipment(new Equipment(dr["EquipmentID"].ToString()));
            });

            _Modules.Modules.Reset(modules);
        }


        /// <summary>
        /// 製品価格を復元
        /// </summary>
        /// <param name="conn"></param>
        private void RestoreProductsPrice(DBConnection conn)
        {
            foreach (var product in _Products.Products)
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
            foreach (var resource in _Resources.Resources)
            {
                conn.ExecQuery($"SELECT Price FROM BuildResources WHERE WareID = '{resource.Ware.WareID}'", (SQLiteDataReader dr, object[] _) =>
                {
                    resource.UnitPrice = (long)dr["Price"];
                });
            }
        }
    }
}

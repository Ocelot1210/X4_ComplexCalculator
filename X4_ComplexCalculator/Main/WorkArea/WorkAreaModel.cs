using Microsoft.Win32;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.ResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.SaveDataReader;
using X4_ComplexCalculator.Main.WorkArea.StorageAssign;

namespace X4_ComplexCalculator.Main.WorkArea
{
    /// <summary>
    /// 作業エリア用Model
    /// </summary>
    class WorkAreaModel : BindableBase, IDisposable
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
        /// 保管庫割当
        /// </summary>
        private readonly StorageAssignModel _StorageAssignModel;


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
                    RaisePropertyChanged();

                    if (value)
                    {
                        // 変更検知イベントを購読解除
                        _Modules.Modules.CollectionChanged -= OnModulesChanged;
                        _Modules.Modules.CollectionPropertyChanged -= OnPropertyChanged;
                        _Products.Products.CollectionPropertyChanged -= OnPropertyChanged;
                        _Resources.Resources.CollectionPropertyChanged -= OnPropertyChanged;
                        _StorageAssignModel.StorageAssignGridItems.CollectionPropertyChanged -= OnPropertyChanged;
                    }
                    else
                    {
                        // 変更検知イベントを購読
                        _Modules.Modules.CollectionChanged += OnModulesChanged;
                        _Modules.Modules.CollectionPropertyChanged += OnPropertyChanged;
                        _Products.Products.CollectionPropertyChanged += OnPropertyChanged;
                        _Resources.Resources.CollectionPropertyChanged += OnPropertyChanged;
                        _StorageAssignModel.StorageAssignGridItems.CollectionPropertyChanged += OnPropertyChanged;
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
        /// <param name="storageAssignModel">保管庫割当</param>
        public WorkAreaModel(ModulesGridModel modules, ProductsGridModel products, ResourcesGridModel resources, StorageAssignModel storageAssignModel)
        {
            _Modules = modules;
            _Products = products;
            _Resources = resources;
            _StorageAssignModel = storageAssignModel;

            HasChanged = true;
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            // 変更検知イベントを購読解除
            _Modules.Modules.CollectionChanged -= OnModulesChanged;
            _Modules.Modules.CollectionPropertyChanged -= OnPropertyChanged;
            _Products.Products.CollectionPropertyChanged -= OnPropertyChanged;
            _Resources.Resources.CollectionPropertyChanged -= OnPropertyChanged;
            _StorageAssignModel.StorageAssignGridItems.CollectionPropertyChanged -= OnPropertyChanged;
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
                nameof(StorageAssignGridItem.AllocCount)
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
            conn.ExecQuery("DROP TABLE IF EXISTS Common");
            conn.ExecQuery("DROP TABLE IF EXISTS Modules");
            conn.ExecQuery("DROP TABLE IF EXISTS Equipments");
            conn.ExecQuery("DROP TABLE IF EXISTS Products");
            conn.ExecQuery("DROP TABLE IF EXISTS BuildResources");
            conn.ExecQuery("DROP TABLE IF EXISTS StorageAssign");
            conn.ExecQuery("CREATE TABLE Common(Item TEXT, Value INTEGER)");
            conn.ExecQuery("CREATE TABLE Modules(Row INTEGER, ModuleID TEXT, Count INTEGER)");
            conn.ExecQuery("CREATE TABLE Equipments(Row INTEGER, EquipmentID TEXT)");
            conn.ExecQuery("CREATE TABLE Products(WareID TEXT, Price INTEGER)");
            conn.ExecQuery("CREATE TABLE BuildResources(WareID TEXT, Price INTEGER)");
            conn.ExecQuery("CREATE TABLE StorageAssign(WareID TEXT, AllocCount INTEGER)");

            // ファイルフォーマットのバージョン保存
            conn.ExecQuery($"INSERT INTO Common(Item, Value) VALUES('FormatVersion', 1)");


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

            // 保管庫割当情報保存
            foreach (var assign in _StorageAssignModel.StorageAssignGridItems)
            {
                conn.ExecQuery($"INSERT INTO StorageAssign(WareID, AllocCount) Values('{assign.WareID}', {assign.AllocCount})");
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
            var reader = SaveDataReaderFactory.CreateSaveDataReader(path, _Modules.Modules, _Products.Products, _Resources.Resources, _StorageAssignModel.StorageAssignGridItems);
            
            // 読み込み成功？
            if (reader.Load())
            {
                SaveFilePath = path;
                HasChanged = false;
            }
        }
    }
}

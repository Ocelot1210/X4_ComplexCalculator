using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using Dapper;
using Microsoft.Win32;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataWriter
{
    /// <summary>
    /// SQLite形式の保存ファイル作成クラス
    /// </summary>
    class SQLiteSaveDataWriter : ISaveDataWriter
    {
        /// <summary>
        /// 保存ファイルパス
        /// </summary>
        public string SaveFilePath { get; set; } = "";


        /// <summary>
        /// 上書き保存
        /// </summary>
        /// <param name="WorkArea">作業エリア</param>
        public bool Save(IWorkArea WorkArea)
        {
            if (string.IsNullOrEmpty(SaveFilePath))
            {
                return SaveAs(WorkArea);
            }

            try
            {
                SaveMain(WorkArea);
                return true;
            }
            catch (Exception e)
            {
                LocalizedMessageBox.Show("Lang:SaveDataWriteFailureMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, e.Message, e.StackTrace ?? "");
            }

            return false;
        }


        /// <summary>
        /// 名前を付けて保存
        /// </summary>
        /// <param name="WorkArea">作業エリア</param>
        public bool SaveAs(IWorkArea WorkArea)
        {
            var dlg = new SaveFileDialog();

            dlg.Filter = "X4 Station calculator data (*.x4)|*.x4|All Files|*.*";
            if (dlg.ShowDialog() == true)
            {
                SaveFilePath = dlg.FileName;

                try
                {
                    SaveMain(WorkArea);
                    WorkArea.Title = Path.GetFileNameWithoutExtension(SaveFilePath);
                    return true;
                }
                catch (Exception e)
                {
                    LocalizedMessageBox.Show("Lang:SaveDataWriteFailureMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, e.Message, e.StackTrace ?? "");
                }
            }


            return false;
        }


        /// <summary>
        /// 保存処理メイン
        /// </summary>
        /// <param name="WorkArea"></param>
        private void SaveMain(IWorkArea WorkArea)
        {
            // フォルダが無ければ作る
            Directory.CreateDirectory(Path.GetDirectoryName(SaveFilePath));

            using var conn = new DBConnection(SaveFilePath);

            conn.BeginTransaction();

            // 保存用テーブル初期化
            conn.Execute("DROP TABLE IF EXISTS Common");
            conn.Execute("DROP TABLE IF EXISTS Modules");
            conn.Execute("DROP TABLE IF EXISTS Equipments");
            conn.Execute("DROP TABLE IF EXISTS Products");
            conn.Execute("DROP TABLE IF EXISTS BuildResources");
            conn.Execute("DROP TABLE IF EXISTS StorageAssign");
            conn.Execute("DROP TABLE IF EXISTS StationSettings");
            conn.Execute("CREATE TABLE Common(Item TEXT, Value INTEGER)");
            conn.Execute("CREATE TABLE Modules(Row INTEGER, ModuleID TEXT, Count INTEGER)");
            conn.Execute("CREATE TABLE Equipments(Row INTEGER, EquipmentID TEXT)");
            conn.Execute("CREATE TABLE Products(WareID TEXT, Price INTEGER, NoBuy INTEGER, NoSell INTEGER)");
            conn.Execute("CREATE TABLE BuildResources(WareID TEXT, Price INTEGER, NoBuy INTEGER)");
            conn.Execute("CREATE TABLE StorageAssign(WareID TEXT, AllocCount INTEGER)");
            conn.Execute("CREATE TABLE StationSettings(Key TEXT, Value TEXT)");


            // ファイルフォーマットのバージョン保存
            conn.Execute("INSERT INTO Common(Item, Value) VALUES('FormatVersion', 2)");

            // ステーションの設定保存
            conn.Execute("INSERT INTO StationSettings(Key, Value) VALUES('IsHeadquarters', :IsHeadquarters)", WorkArea.StationData.Settings);
            conn.Execute("INSERT INTO StationSettings(Key, Value) VALUES('Sunlight', :Sunlight)", WorkArea.StationData.Settings);
            conn.Execute("INSERT INTO StationSettings(Key, Value) VALUES('ActualWorkforce', :Actual)", WorkArea.StationData.Settings.Workforce);
            conn.Execute("INSERT INTO StationSettings(Key, Value) VALUES('AlwaysMaximumWorkforce', :AlwaysMaximum)", WorkArea.StationData.Settings.Workforce);

            // モジュール保存
            foreach (var (module, i) in WorkArea.StationData.ModulesInfo.Modules.Select((module, i) => (module, i)))
            {
                const string sql1 = "INSERT INTO Modules(Row, ModuleID, Count) Values(:i, :ModuleID, :ModuleCount)";
                conn.Execute(sql1, new { i, module.Module.ID, module.ModuleCount });

                if (module.EditStatus == EditStatus.Edited)
                {
                    module.EditStatus |= EditStatus.Saved;
                }

                const string sql2 = "INSERT INTO Equipments(Row, EquipmentID) Values(:i, :EquipmentID)";
                var param2 = module.Equipments.AllEquipments.Select(e => new { i, e.ID });
                conn.Execute(sql2, param2);
            }

            // 価格保存
            SqlMapper.AddTypeHandler(new WareTypeHandler());
            foreach (var product in WorkArea.StationData.ProductsInfo.Products)
            {
                if (product.EditStatus == EditStatus.Edited)
                {
                    product.EditStatus |= EditStatus.Saved;
                }

                const string sql = "INSERT INTO Products(WareID, Price, NoBuy, NoSell) Values(:Ware, :UnitPrice, :NoBuy, :NoSell)";
                conn.Execute(sql, product);
            }

            // 建造リソース保存
            foreach (var resource in WorkArea.StationData.BuildResourcesInfo.BuildResources)
            {
                const string sql = "INSERT INTO BuildResources(WareID, Price, NoBuy) Values(:Ware, :UnitPrice, :NoBuy)";
                conn.Execute(sql, resource);

                if (resource.EditStatus == EditStatus.Edited)
                {
                    resource.EditStatus |= EditStatus.Saved;
                }
            }

            // 保管庫割当情報保存
            foreach (var assign in WorkArea.StationData.StorageAssignInfo.StorageAssign)
            {
                const string sql = "INSERT INTO StorageAssign(WareID, AllocCount) Values(:WareID, :AllocCount)";
                conn.Execute(sql, assign);

                if (assign.EditStatus == EditStatus.Edited)
                {
                    assign.EditStatus |= EditStatus.Saved;
                }
            }

            conn.Commit();
        }


        /// <summary>
        /// Dappar が Ware を WareID に変換するためのクラス
        /// </summary>
        private class WareTypeHandler : SqlMapper.TypeHandler<Ware>
        {
            /// <inheritdoc />
            public override Ware Parse(object value) => throw new NotImplementedException();


            /// <inheritdoc />
            public override void SetValue(IDbDataParameter parameter, Ware value)
            {
                parameter.DbType = DbType.String;
                parameter.Value = value.ID;
            }
        }
    }
}

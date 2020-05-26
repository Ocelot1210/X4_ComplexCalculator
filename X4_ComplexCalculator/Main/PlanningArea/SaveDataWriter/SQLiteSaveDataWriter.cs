using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB;

namespace X4_ComplexCalculator.Main.PlanningArea.SaveDataWriter
{
    class SQLiteSaveDataWriter : ISaveDataWriter
    {
        /// <summary>
        /// 保存ファイルパス
        /// </summary>
        public string SaveFilePath { get; set; } = "";


        /// <summary>
        /// 上書き保存
        /// </summary>
        /// <param name="PlanningArea">作業エリア</param>
        public bool Save(IPlanningArea PlanningArea)
        {
            if (string.IsNullOrEmpty(SaveFilePath))
            {
                return SaveAs(PlanningArea);
            }

            try
            {
                SaveMain(PlanningArea);
                return true;
            }
            catch (Exception e)
            {
                Localize.ShowMessageBox("Lang:SaveDataWriteFailureMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, e.Message, e.StackTrace);
            }

            return false;
        }


        /// <summary>
        /// 名前を付けて保存
        /// </summary>
        /// <param name="PlanningArea">作業エリア</param>
        public bool SaveAs(IPlanningArea PlanningArea)
        {
            var ret = false;

            var dlg = new SaveFileDialog();

            dlg.Filter = "X4 Station calclator data (*.x4)|*.x4|All Files|*.*";
            if (dlg.ShowDialog() == true)
            {
                SaveFilePath = dlg.FileName;

                try
                {
                    SaveMain(PlanningArea);
                    PlanningArea.Title = Path.GetFileNameWithoutExtension(SaveFilePath);
                    ret = true;
                }
                catch(Exception e)
                {
                    Localize.ShowMessageBox("Lang:SaveDataWriteFailureMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, e.Message, e.StackTrace);
                }
            }


            return ret;
        }


        /// <summary>
        /// 保存処理メイン
        /// </summary>
        /// <param name="PlanningArea"></param>
        private void SaveMain(IPlanningArea PlanningArea)
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
            foreach (var module in PlanningArea.Modules)
            {
                conn.ExecQuery($"INSERT INTO Modules(Row, ModuleID, Count) Values({rowCnt}, '{module.Module.ModuleID}', {module.ModuleCount})");

                foreach (var equipment in module.Module.Equipment.GetAllEquipment())
                {
                    conn.ExecQuery($"INSERT INTO Equipments(Row, EquipmentID) Values({rowCnt}, '{equipment.EquipmentID}')");
                }

                rowCnt++;
            }


            // 価格保存
            foreach (var product in PlanningArea.Products)
            {
                conn.ExecQuery($"INSERT INTO Products(WareID, Price) Values('{product.Ware.WareID}', {product.UnitPrice})");
            }

            // 建造リソース保存
            foreach (var resource in PlanningArea.Resources)
            {
                conn.ExecQuery($"INSERT INTO BuildResources(WareID, Price) Values('{resource.Ware.WareID}', {resource.UnitPrice})");
            }

            // 保管庫割当情報保存
            foreach (var assign in PlanningArea.StorageAssign)
            {
                conn.ExecQuery($"INSERT INTO StorageAssign(WareID, AllocCount) Values('{assign.WareID}', {assign.AllocCount})");
            }

            conn.Commit();
        }
    }
}

using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using X4_ComplexCalculator.DB;

namespace X4_ComplexCalculator.Main.WorkArea.SaveDataWriter
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
        /// <param name="workArea">作業エリア</param>
        public bool Save(IWorkArea workArea)
        {
            if (string.IsNullOrEmpty(SaveFilePath))
            {
                return SaveAs(workArea);
            }

            try
            {
                SaveMain(workArea);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"保存に失敗しました。\r\n■理由：\r\n{e.Message}\r\n\r\n■スタックトレース：\r\n{e.StackTrace}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }


        /// <summary>
        /// 名前を付けて保存
        /// </summary>
        /// <param name="workArea">作業エリア</param>
        public bool SaveAs(IWorkArea workArea)
        {
            var ret = false;

            var dlg = new SaveFileDialog();

            dlg.Filter = "X4 Station calclator data (*.x4)|*.x4|All Files|*.*";
            if (dlg.ShowDialog() == true)
            {
                SaveFilePath = dlg.FileName;

                try
                {
                    SaveMain(workArea);
                    workArea.Title = Path.GetFileNameWithoutExtension(SaveFilePath);
                    ret = true;
                }
                catch(Exception e)
                {
                    MessageBox.Show($"保存に失敗しました。\r\n■理由：\r\n{e.Message}\r\n\r\n■スタックトレース：\r\n{e.StackTrace}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }


            return ret;
        }


        /// <summary>
        /// 保存処理メイン
        /// </summary>
        /// <param name="workArea"></param>
        private void SaveMain(IWorkArea workArea)
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
            foreach (var module in workArea.Modules)
            {
                conn.ExecQuery($"INSERT INTO Modules(Row, ModuleID, Count) Values({rowCnt}, '{module.Module.ModuleID}', {module.ModuleCount})");

                foreach (var equipment in module.Module.Equipment.GetAllEquipment())
                {
                    conn.ExecQuery($"INSERT INTO Equipments(Row, EquipmentID) Values({rowCnt}, '{equipment.EquipmentID}')");
                }

                rowCnt++;
            }


            // 価格保存
            foreach (var product in workArea.Products)
            {
                conn.ExecQuery($"INSERT INTO Products(WareID, Price) Values('{product.Ware.WareID}', {product.UnitPrice})");
            }

            // 建造リソース保存
            foreach (var resource in workArea.Resources)
            {
                conn.ExecQuery($"INSERT INTO BuildResources(WareID, Price) Values('{resource.Ware.WareID}', {resource.UnitPrice})");
            }

            // 保管庫割当情報保存
            foreach (var assign in workArea.StorageAssign)
            {
                conn.ExecQuery($"INSERT INTO StorageAssign(WareID, AllocCount) Values('{assign.WareID}', {assign.AllocCount})");
            }

            conn.Commit();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.XPath;
using LibX4.FileSystem;
using LibX4.Lang;
using X4_DataExporterWPF.Export;

namespace X4_DataExporterWPF.DataExportWindow
{
    /// <summary>
    /// データ抽出処理用Model
    /// </summary>
    class DataExportModel
    {
        /// <summary>
        /// 言語一覧を更新
        /// </summary>
        public (bool success, IEnumerable<LangComboboxItem> languages) GetLanguages(string inDirPath)
        {
            try
            {
                var catFiles = new CatFile(inDirPath);
                var xml = catFiles.OpenXml("libraries/languages.xml");
                var languages = xml.XPathSelectElements("/languages/language")
                    .Select(x => new LangComboboxItem(int.Parse(x.Attribute("id").Value), x.Attribute("name").Value))
                    .OrderBy(x => x.ID);

                return (true, languages);
            }
            catch (Exception)
            {
                return (false, Enumerable.Empty<LangComboboxItem>());
            }
        }


        /// <summary>
        /// 抽出実行
        /// </summary>
        /// <param name="inDirPath">入力元フォルダパス</param>
        /// <param name="outFilePath">出力先ファイルパス</param>
        /// <param name="language">選択された言語</param>
        /// <param name="owner">親ウィンドウハンドル(メッセージボックス表示用)</param>
        /// <returns>現在数と合計数のタプルのイテレータ</returns>
        public void Export(IProgress<(int currentStep, int maxSteps)> progless, string inDirPath, string outFilePath, LangComboboxItem language, Window owner)
        {
            var catFile = new CatFile(inDirPath);

            // 抽出に失敗した場合、どこで例外が発生したか知りたいため、Debugビルドではtry-catchを無効化する
#if !DEBUG
            try
#endif
            {
                if (File.Exists(outFilePath))
                {
                    File.Delete(outFilePath);
                }

                var consb = new SQLiteConnectionStringBuilder { DataSource = outFilePath };
                using var conn = new SQLiteConnection(consb.ToString());
                conn.Open();

                using var trans = conn.BeginTransaction();

                // 英語をデフォルトにする
                var resolver = new LanguageResolver(catFile, language.ID, 44);

                var factionsXml = catFile.OpenXml("libraries/factions.xml");
                var raceXml = catFile.OpenXml("libraries/races.xml");
                var waresXml = catFile.OpenXml("libraries/wares.xml");
                //var mapXml = catFile.OpenXml("libraries/mapdefaults.xml");
                var wareGroupXml = catFile.OpenXml("libraries/waregroups.xml");

                IExporter[] exporters =
                {
                    // 共通
                    new CommonExporter(),                               // 共通情報
                    new EffectExporter(),                               // 追加効果情報
                    new SizeExporter(resolver),                         // サイズ情報
                    new TransportTypeExporter(resolver),                // カーゴ種別情報
                    new RaceExporter(raceXml, resolver),                // 種族情報
                    new FactionExporter(factionsXml, resolver),         // 派閥情報
                    new PurposeExporter(resolver),                      // 用途情報
                    //new MapExporter(mapXml, resolver),                 // マップ

                    // ウェア関連
                    new WareGroupExporter(wareGroupXml, resolver),      // ウェア種別情報
                    new WareExporter(waresXml, resolver),               // ウェア情報
                    new WareResourceExporter(waresXml),                 // ウェア生産時に必要な情報
                    new WareProductionExporter(waresXml, resolver),     // ウェア生産に必要な情報
                    new WareEffectExporter(waresXml),                   // ウェア生産時の追加効果情報

                    // モジュール関連
                    new ModuleTypeExporter(resolver),                   // モジュール種別情報
                    new ModuleExporter(catFile, waresXml, resolver),    // モジュール情報
                    new ModuleOwnerExporter(waresXml),                  // モジュール所有派閥情報
                    new ModuleProductionExporter(waresXml),             // モジュール建造情報
                    new ModuleResourceExporter(waresXml),               // モジュール建造に必要なウェア情報
                    new ModuleProductExporter(catFile, waresXml),       // モジュールの生産品情報
                    new ModuleShieldExporter(catFile, waresXml),        // モジュールのシールド情報
                    new ModuleTurretExporter(catFile, waresXml),        // モジュールのタレット情報
                    new ModuleStorageExporter(catFile, waresXml),       // モジュールの保管容量情報

                    // 装備関連
                    new EquipmentTypeExporter(resolver),                // 装備種別情報
                    new EquipmentExporter(catFile, waresXml, resolver), // 装備情報
                    new EquipmentOwnerExporter(waresXml),               // 装備保有派閥情報
                    new EquipmentResourceExporter(waresXml),            // 装備生産に必要なウェア情報
                    new EquipmentProductionExporter(waresXml),          // 装備生産に関する情報

                    // 従業員関連
                    new WorkUnitProductionExporter(waresXml),           // 従業員用生産情報
                    new WorkUnitResourceExporter(waresXml),             // 従業員用必要ウェア情報

                    // 艦船関連
                    new ShipTypeExporter(resolver),                     // 艦船種別情報
                    new ShipExporter(catFile, waresXml, resolver),      // 艦船情報
                    new ShipPurposeExporter(catFile, waresXml),         // 艦船用途情報
                    new ShipEquipmentExporter(catFile, waresXml),       // 艦船装備情報
                    new ShipHangerExporter(catFile, waresXml),          // 艦船ハンガー(機体格納庫)情報
                    new ShipTransportTypeExporter(catFile, waresXml),   // 艦船カーゴ情報
                };

                // 進捗初期化
                var maxSteps = exporters.Length;
                var currentStep = 0;
                foreach (var exporter in exporters)
                {
                    exporter.Export(conn);
                    currentStep++;
                    progless.Report((currentStep, maxSteps));
                }

                trans.Commit();
                owner.Dispatcher.BeginInvoke((Action)(() =>
                {
                    MessageBox.Show("Data export completed.", "X4 DataExporter", MessageBoxButton.OK, MessageBoxImage.Information);
                }));
            }
#if !DEBUG
            catch (Exception e)
            {
                // テンポラリフォルダにクラッシュレポートをダンプする
                var dumpPath = Path.Combine(Path.GetTempPath(), "X4_ComplexCalculator_CrashReport.txt");

                DumpCrashReport(dumpPath, catFile, e);

                var msg = @$"Sorry, Data export failed.
Please report the following content to the developer.

1. Selected language.
2. Crash report file.
3. Version of X4.";

                owner.Dispatcher.BeginInvoke((Action)(() =>
                {
                    MessageBox.Show(owner, msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    System.Diagnostics.Process.Start("explorer.exe", $@"/select,""{dumpPath}""");
                }));
            }
#endif
        }


        /// <summary>
        /// クラッシュレポートをダンプする
        /// </summary>
        /// <param name="path"></param>
        /// <param name="catFile"></param>
        /// <param name="e"></param>
        private void DumpCrashReport(string path, CatFile catFile, Exception e)
        {
            using var sw = new StreamWriter(path);

            sw.WriteLine("Sorry, Data export failed.");
            sw.WriteLine("Please report the following content to the developer.");
            sw.WriteLine("");
            sw.WriteLine("1. Selected language.");
            sw.WriteLine("2. Crash report file. (this file)");
            sw.WriteLine("3. Version of X4.");

            sw.WriteLine("\r\n");

            sw.WriteLine("----------------------------------------------------------------------");
            sw.WriteLine("Exception information");
            sw.WriteLine("----------------------------------------------------------------------");
            sw.WriteLine($"■ Type :\r\n{e.GetType()}\r\n");
            sw.WriteLine($"■ Message :\r\n{e.Message}\r\n");
            sw.WriteLine($"■ StackTrace :\r\n{e.StackTrace}\r\n");

            // Modがインストールされていればその一覧を出力する
            if (catFile.IsModInstalled)
            {
                sw.WriteLine("\r\n\r\n\r\n");

                sw.WriteLine("----------------------------------------------------------------------");
                sw.WriteLine("Installed mods");
                sw.WriteLine("----------------------------------------------------------------------");
                catFile.DumpModInfo(sw);
            }
        }
    }
}

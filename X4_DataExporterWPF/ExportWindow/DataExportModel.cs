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
        public IEnumerable<LangComboboxItem> GetLangages(string inDirPath)
        {
            try
            {
                var catFiles = new CatFile(inDirPath);
                var xml = catFiles.OpenXml("libraries/languages.xml");
                var langages = xml.XPathSelectElements("/languages/language")
                    .Select(x => new LangComboboxItem(int.Parse(x.Attribute("id").Value), x.Attribute("name").Value))
                    .OrderBy(x => x.ID);

                return langages;
            }
            catch (Exception)
            {
                return Enumerable.Empty<LangComboboxItem>();
            }
        }


        /// <summary>
        /// 抽出実行
        /// </summary>
        /// <param name="inDirPath">入力元フォルダパス</param>
        /// <param name="outFilePath">出力先ファイルパス</param>
        /// <param name="language">選択された言語</param>
        /// <returns>現在数と合計数のタプルのイテレータ</returns>
        public void Export(IProgress<(int currentStep, int maxSteps)> progless, string inDirPath, string outFilePath, LangComboboxItem language)
        {
            // 抽出に失敗した場合、どこで例外が発生したか知りたいため、Debugビルドではtry-catchを無効化する
#if !DEBUG
            try
#endif
            {

                if (File.Exists(outFilePath))
                {
                    File.Delete(outFilePath);
                }

                var catFile = new CatFile(inDirPath);

                var consb = new SQLiteConnectionStringBuilder { DataSource = outFilePath };
                using var conn = new SQLiteConnection(consb.ToString());
                conn.Open();

                using var trans = conn.BeginTransaction();

                var resolver = new LanguageResolver(catFile);

                // 英語をデフォルトにする
                resolver.LoadLangFile(44);
                resolver.LoadLangFile(language.ID);


                var waresXml = catFile.OpenXml("libraries/wares.xml");

                IExporter[] exporters =
                {
                    // 共通
                    new CommonExporter(),                               // 共通情報
                    new EffectExporter(),                               // 追加効果情報
                    new SizeExporter(resolver),                         // サイズ情報
                    new TransportTypeExporter(resolver),                // カーゴ種別情報
                    new RaceExporter(catFile, resolver),                // 種族情報
                    new FactionExporter(catFile, resolver),             // 派閥情報
                    //new MapExporter(catFile, resolver),                 // マップ

                    // ウェア関連
                    new WareGroupExporter(catFile, resolver),           // ウェア種別情報
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
                    new WorkUnitResourceExporter(waresXml)              // 従業員用必要ウェア情報
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

                MessageBox.Show("Data export completed.", "X4 DataExporter", MessageBoxButton.OK, MessageBoxImage.Information);
            }
#if !DEBUG
            catch (Exception e)
            {
                var msg = $"■Message\r\n{e.Message}\r\n\r\n■StackTrace\r\n{e.StackTrace}";
                MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
#endif
        }
    }
}

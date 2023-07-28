using LibX4.FileSystem;
using LibX4.Lang;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using System.Xml.XPath;
using WPFLocalizeExtension.Engine;
using X4_DataExporterWPF.Export;

namespace X4_DataExporterWPF.DataExportWindow;

/// <summary>
/// データ抽出処理用Model
/// </summary>
class DataExportModel
{
    /// <summary>
    /// 言語一覧を更新
    /// </summary>
    /// <param name="x4Dir">X4のインストール先フォルダ</param>
    /// <param name="catLoadOption">cat ファイルの読み込みオプション</param>
    /// <param name="owner">親ウィンドウハンドル(メッセージボックス表示用)</param>
    public static async Task<(bool success, IReadOnlyList<LangComboboxItem> languages)> GetLanguages(string x4Dir, CatLoadOption catLoadOption, Window owner)
    {
        try
        {
            var catFiles = new CatFile(x4Dir, catLoadOption);
            var xml = await catFiles.OpenXmlAsync("libraries/languages.xml", CancellationToken.None);
            var languages = xml.XPathSelectElements("/languages/language")
                .Select(x => (ID: x.Attribute("id")?.Value, Name: x.Attribute("name")?.Value))
                .Where(x => (!string.IsNullOrEmpty(x.ID)) && x.Name is not null)
                .Select(x => new LangComboboxItem(int.Parse(x.ID!), x.Name!))
                .OrderBy(x => x.ID)
                .ToArray();

            return (true, languages);
        }
        catch (DependencyResolutionException ex)
        {
            await owner.Dispatcher.BeginInvoke(() =>
            {
                static void AddModInfo(StringBuilder sb, ModInfo modInfo, int level = 0)
                {
                    if (level == 0)
                    {
                        sb.AppendLine(modInfo.Name);
                    }

                    string indent = "".PadRight((level + 1) * 4);

                    foreach (var dependency in modInfo.Dependencies)
                    {
                        // dependency.ModInfo が null 以外の場合も出力する。
                        // → 依存関係が循環していると Mod の詳細情報 (dependency.ModInfo) が 非 null になるため。

                        sb.Append(indent);
                        sb.AppendLine(dependency.ModInfo?.Name ?? dependency.Name);
                        if (dependency.ModInfo is not null)
                        {
                            AddModInfo(sb, dependency.ModInfo, level + 1);
                        }
                    }
                }

                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("---------");
                foreach (var mod in ex.UnloadedMods)
                {
                    AddModInfo(sb, mod);
                    sb.AppendLine();
                }

                var msg = (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:DataExporter_FailedToResolveModDependencyMessage", null, null);
                var title = (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:DataExporter_Title", null, null);

                MessageBox.Show(owner, $"{msg}\r\n{sb}", title, MessageBoxButton.OK, MessageBoxImage.Error);
            });
            return (false, Array.Empty<LangComboboxItem>());
        }
        catch (Exception)
        {
            return (false, Array.Empty<LangComboboxItem>());
        }
    }


    /// <summary>
    /// 抽出実行
    /// </summary>
    /// <param name="inDirPath">入力元フォルダパス</param>
    /// <param name="catLoadOption">cat ファイルの読み込みオプション</param>
    /// <param name="outFilePath">出力先ファイルパス</param>
    /// <param name="language">選択された言語</param>
    /// <param name="owner">親ウィンドウハンドル(メッセージボックス表示用)</param>
    /// <returns>現在数と合計数のタプルのイテレータ</returns>
    public static async Task Export(
        IProgress<(int currentStep, int maxSteps)> progress,
        IProgress<(int currentStep, int maxSteps)> progressSub,
        string inDirPath,
        CatLoadOption catLoadOption,
        string outFilePath,
        LangComboboxItem language,
        Window owner
    )
    {
        var catFile = new CatFile(inDirPath, catLoadOption);

        // 抽出に失敗した場合、例外設定で「Common Languate Runtime Exceptions」にチェックを入れるとどこで例外が発生したか分かる
        try
        {
            using var backupper = new DbBackupper(outFilePath);

            var consb = new SQLiteConnectionStringBuilder { DataSource = outFilePath };
            using var conn = new SQLiteConnection(consb.ToString());
            conn.Open();

            using var trans = conn.BeginTransaction();

            // 英語をデフォルトにする
            var resolver = await LanguageResolver.CreateAsync(catFile, language.ID, 44);

            var waresXml = await catFile.OpenXmlAsync("libraries/wares.xml");
            RemoveDuplicateWares(waresXml);

            var defaultXml = await catFile.OpenXmlAsync("libraries/defaults.xml");

            IExporter[] exporters =
            {
                // 共通
                new CommonExporter(),                                       // 共通情報
                new EffectExporter(),                                       // 追加効果情報
                new SizeExporter(resolver),                                 // サイズ情報
                new TransportTypeExporter(waresXml, resolver),              // カーゴ種別情報
                new RaceExporter(catFile, resolver),                        // 種族情報
                new FactionExporter(catFile, resolver),                     // 派閥情報
                new PurposeExporter(catFile, waresXml, resolver),           // 用途情報
                //new MapExporter(mapXml, resolver),                        // マップ

                // ウェア関連
                new WareGroupExporter(catFile, resolver),                   // ウェア種別情報
                new WareExporter(waresXml, resolver),                       // ウェア情報
                new WareResourceExporter(waresXml),                         // ウェア生産時に必要な情報
                new WareProductionExporter(waresXml, resolver),             // ウェア生産に必要な情報
                new WareEffectExporter(waresXml),                           // ウェア生産時の追加効果情報
                new WareOwnerExporter(waresXml),                            // ウェア所有派閥情報
                new WareEquipmentExporter(catFile, waresXml),               // ウェアの装備情報
                new WareTagsExporter(waresXml),                             // ウェアのタグ情報

                // モジュール関連
                new ModuleTypeExporter(catFile, waresXml, resolver),        // モジュール種別情報
                new ModuleExporter(catFile, waresXml),                      // モジュール情報
                new ModuleProductExporter(catFile, waresXml, defaultXml),   // モジュールの生産品情報
                new ModuleStorageExporter(catFile, waresXml),               // モジュールの保管容量情報

                // 装備関連
                new EquipmentTypeExporter(waresXml, resolver),              // 装備種別情報
                new EquipmentExporter(catFile, waresXml),                   // 装備情報
                new ShieldExporter(catFile, waresXml),                      // シールド情報
                new EngineExporter(catFile, waresXml),                      // エンジン情報
                new ThrusterExporter(catFile, waresXml),                    // スラスター情報


                // 艦船関連
                new ShipTypeExporter(catFile, waresXml, resolver),          // 艦船種別情報
                new ShipExporter(catFile, waresXml),                        // 艦船情報
                new ShipPurposeExporter(catFile, waresXml),                 // 艦船用途情報
                new ShipHangerExporter(catFile, waresXml),                  // 艦船ハンガー(機体格納庫)情報
                new ShipTransportTypeExporter(catFile, waresXml),           // 艦船カーゴ情報
                new ShipLoadoutExporter(catFile, waresXml),                 // 艦船のロードアウト情報
            };

            // 進捗初期化
            var maxSteps = exporters.Length;
            var currentStep = 0;
            foreach (var exporter in exporters)
            {
                await exporter.ExportAsync(conn, progressSub, CancellationToken.None);
                currentStep++;
                progress.Report((currentStep, maxSteps));
            }

            trans.Commit();
            backupper.Commit();

            await owner.Dispatcher.BeginInvoke((Action)(() =>
            {
                var msg = (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:DataExporter_ExportCompleted", null, null);
                var title = (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:DataExporter_Title", null, null);

                MessageBox.Show(owner, msg, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }));
        }
        catch (DbBackupException)
        {
            await owner.Dispatcher.BeginInvoke((Action)(() =>
            {
                var msg = (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:DataExporter_FailedToBackupDb", null, null);
                var title = (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:DataExporter_Title", null, null);

                MessageBox.Show(owner, msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }
        catch (Exception e)
        {
            // テンポラリフォルダにクラッシュレポートをダンプする
            var dumpPath = Path.Combine(Path.GetTempPath(), "X4_ComplexCalculator_CrashReport.txt");

            DumpCrashReport(dumpPath, catFile, e);

            await owner.Dispatcher.BeginInvoke((Action)(() =>
            {
                var msg = (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:DataExporter_FailedToExportMessage", null, null);
                var title = (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:DataExporter_Title", null, null);

                MessageBox.Show(owner, msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Process.Start("explorer.exe", $@"/select,""{dumpPath}""");
            }));
        }
        finally
        {
            progress.Report((0, 1));
            progressSub.Report((0, 1));
        }
    }


    /// <summary>
    /// wares.xml から重複する要素を削除する(前の要素を消す)
    /// </summary>
    /// <param name="waresXml">削除対象</param>
    private static void RemoveDuplicateWares(XDocument waresXml)
    {
        ArgumentNullException.ThrowIfNull(waresXml.Root);

        // 見つかったウェアID一覧
        var wareIds = new HashSet<string>();

        // 後の要素が優先されるため、 Reverse() する
        foreach (var ware in waresXml.Root.XPathSelectElements("ware").Reverse())
        {
            // ウェアIDが無い又は重複があれば削除する
            var id = ware.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(id) || !wareIds.Add(id))
            {
                ware.Remove();
            }
        }
    }


    /// <summary>
    /// クラッシュレポートをダンプする
    /// </summary>
    /// <param name="path"></param>
    /// <param name="catFile"></param>
    /// <param name="e"></param>
    private static void DumpCrashReport(string path, CatFile catFile, Exception e)
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

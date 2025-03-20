using CommunityToolkit.Mvvm.Messaging;
using LibX4.FileSystem;
using LibX4.Lang;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_DataExporterWPF.Export;

namespace X4_DataExporterWPF.ExportWindows;

/// <summary>
/// データ抽出処理用Model
/// </summary>
class DataExportModel
{
    /// <summary>
    /// メッセージ通知用
    /// </summary>
    private readonly IMessenger _messenger;


    /// <summary>
    /// 出力先ファイルパス
    /// </summary>
    private readonly string _outFilePath;


    /// <summary>
    /// 言語一覧
    /// </summary>
    public ObservableCollection<LangComboboxItem> Languages { get; } = new();


    /// <summary>
    /// 設定フォルダ一覧
    /// </summary>
    public ObservableCollection<string> ConfigFolderPaths { get; } = new();


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messenger">メッセージ通知用</param>
    /// <param name="outFilePath">ファイルパス</param>
    public DataExportModel(IMessenger messenger, string outFilePath)
    {
        _messenger = messenger;
        _outFilePath = outFilePath;

        BindingOperations.EnableCollectionSynchronization(Languages, new object());
        BindingOperations.EnableCollectionSynchronization(ConfigFolderPaths, new object());

        foreach (var path in LibX4.X4Path.EnumerateConfigFolders())
        {
            ConfigFolderPaths.Add(path);
        }
    }


    /// <summary>
    /// 言語一覧を更新
    /// </summary>
    /// <param name="x4Dir">X4のインストール先フォルダ</param>
    /// <param name="configFolderPath">設定フォルダパス</param>
    /// <param name="catLoadOption">cat ファイルの読み込みオプション</param>
    /// <param name="cancellationToken">キャンセル用トークン</param>
    public async Task UpdateLanguagesAsync(string x4Dir, string configFolderPath, CatLoadOption catLoadOption, CancellationToken cancellationToken = default)
    {
        Languages.Clear();

        var catFiles = new CatFile(x4Dir, configFolderPath, catLoadOption);
        var xml = await catFiles.OpenXmlAsync("libraries/languages.xml", cancellationToken);

        var languages = xml.XPathSelectElements("/languages/language")
            .Select(x => (ID: x.Attribute("id")?.Value, Name: x.Attribute("name")?.Value))
            .Where(x => (!string.IsNullOrEmpty(x.ID)) && x.Name is not null)
            .Select(x => new LangComboboxItem(int.Parse(x.ID!), x.Name!))
            .OrderBy(x => x.ID);

        foreach (var item in languages)
        {
            Languages.Add(item);
        }
    }


    /// <summary>
    /// 抽出実行
    /// </summary>
    /// <param name="inDirPath">入力元フォルダパス</param>
    /// <param name="configFolderPath">設定フォルダパス</param>
    /// <param name="catLoadOption">cat ファイルの読み込みオプション</param>
    /// <param name="language">選択された言語</param>
    /// <returns>現在数と合計数のタプルのイテレータ</returns>
    public async Task Export(
        IProgress<(int currentStep, int maxSteps)> progress,
        IProgress<(int currentStep, int maxSteps)> progressSub,
        string inDirPath,
        string configFolderPath,
        CatLoadOption catLoadOption,
        LangComboboxItem language
    )
    {
        var catFile = new CatFile(inDirPath, configFolderPath, catLoadOption);

        // 抽出に失敗した場合、例外設定で「Common Languate Runtime Exceptions」にチェックを入れるとどこで例外が発生したか分かる
        try
        {
            using var backupper = new DbBackupper(_outFilePath);

            var consb = new SQLiteConnectionStringBuilder { DataSource = _outFilePath };
            using var conn = new SQLiteConnection(consb.ToString());
            conn.Open();

            using var trans = conn.BeginTransaction();

            // 英語をデフォルトにする
            var resolver = await LanguageResolver.CreateAsync(catFile, language.ID, 44);

            var waresXml = await catFile.OpenXmlAsync("libraries/wares.xml");
            RemoveDuplicateWares(waresXml);

            var defaultXml = await catFile.OpenXmlAsync("libraries/defaults.xml");

            IExporter[] exporters =
            [
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
            ];

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

            _messenger.Send(Tuple.Create("Lang:DataExporter_ExportCompleted", "Lang:DataExporter_Title"));
        }
        catch (DbBackupException e)
        {
            _messenger.Send(e);
        }
        catch (Exception e)
        {
            // テンポラリフォルダにクラッシュレポートをダンプする
            var dumpPath = Path.Combine(Path.GetTempPath(), "X4_ComplexCalculator_CrashReport.txt");
            DumpCrashReport(dumpPath, catFile, e);

            _messenger.Send(Tuple.Create(e, dumpPath));
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

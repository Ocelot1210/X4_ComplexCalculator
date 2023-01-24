using LibX4.FileSystem;
using LibX4.Lang;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using X4_DataExporterWPF.Entity;
using X4_DataExporterWPF.Export;
using Xunit;

namespace X4_DataExporterWPF.Tests;

public class ExporterTest
{
private static readonly IProgress<(int, int)>? pg = null;
    private static readonly CancellationToken ct = default;

    /// <summary>
    /// モジュールの最大収容人数が省略されている場合、0 として扱う。
    /// </summary>
    [Fact]
    public void ModuleIfOmitMaxWorkers()
    {
        var wareXml = @"
            <wares>
                <ware id=""module_arg_hab_l_01""  name=""{20104,30301}"" tags=""module"">
                    <component ref=""hab_arg_l_01_macro"" />
                </ware>
            </wares>
            ".ToXDocument();
        var macroXml = @"
            <macros>
                <macro class=""habitation"">
                    <properties>
                        <workforce capacity=""1000"" />
                    </properties>
                </macro>
            </macros>
            ".ToXDocument();
        var exporter = new ModuleExporter(new DummyCat(macroXml), wareXml);

        Assert.Equal(exporter.GetRecordsAsync(pg, ct).ToEnumerable(), new[] { new Module(
            ModuleID: "module_arg_hab_l_01",
            ModuleTypeID: "habitation",
            Macro: "hab_arg_l_01_macro",
            MaxWorkers: 0,
            WorkersCapacity: 1000,
            NoBlueprint: false,
            Thumbnail: null
        )});
    }


    /// <summary>
    /// モジュールの従業員数が省略されている場合、0 として扱う。
    /// </summary>
    [Fact]
    public void ModuleIfOmitCapacity()
    {
        var wareXml = @"
            <wares>
                <ware id=""module_arg_prod_foodrations_01"" name=""{20104,13401}"" tags=""module"">
                    <component ref=""prod_arg_foodrations_macro"" />
                </ware>
            </wares>
            ".ToXDocument();
        var macroXml = @"
            <macros>
                <macro class=""production"">
                    <properties>
                        <workforce max=""90"" />
                    </properties>
                </macro>
            </macros>
            ".ToXDocument();
        var exporter = new ModuleExporter(new DummyCat(macroXml), wareXml);

        Assert.Equal(exporter.GetRecordsAsync(pg, ct).ToEnumerable(), new[] { new Module(
            ModuleID: "module_arg_prod_foodrations_01",
            ModuleTypeID: "production",
            Macro: "prod_arg_foodrations_macro",
            MaxWorkers: 90,
            WorkersCapacity: 0,
            NoBlueprint: false,
            Thumbnail: null
        )});
    }


    /// <summary>
    /// 重複する WareOwner は無視する
    /// 参照: <a href="https://github.com/Ocelot1210/X4_ComplexCalculator/pull/6">#6</a>
    /// </summary>
    [Fact]
    public void WareOwnerIgnoreDuplicates()
    {
        var xml = @"
                <wares>
                    <ware id=""module_arg_conn_base_01"" tags=""module"">
                        <owner faction=""antigone"" />
                        <owner faction=""antigone"" />
                    </ware>
                </wares>
            ".ToXDocument();
        var exporter = new WareOwnerExporter(xml);

        Assert.Equal(exporter.GetRecords(), new[] { new WareOwner(
            WareID: "module_arg_conn_base_01",
            FactionID: "antigone"
        )});
    }


    /// <summary>
    /// WareEffect#Product の値が double に変換できない場合、0.0 として扱い続行する
    /// 参照: <a href="https://github.com/Ocelot1210/X4_ComplexCalculator/pull/7">#7</a>
    /// </summary>
    /// <remarks>TODO: 具体的な状況は不明。要調査</remarks>
    [Fact]
    public void WareEffectIfProductParsingFailsContinueAsDefault()
    {
        var xml = @"
                <wares>
                    <ware id=""advancedcomposites"" tags=""economy"">
                        <production method=""default"">
                            <effects>
                                <effect type=""work"" product=""ILLEGAL"" />
                            </effects>
                        </production>
                    </ware>
                </wares>
            ".ToXDocument();
        var exporter = new WareEffectExporter(xml);

        Assert.Equal(exporter.GetRecords(pg, ct), new[] { new WareEffect(
            WareID: "advancedcomposites",
            Method: "default",
            EffectID: "work",
            Product: 0.0
        )});
    }


    /// <summary>
    /// ウェア種別の階級が省略されている場合、0 として扱う。
    /// </summary>
    [Fact]
    public void WareGroupIfOmitTier()
    {
        var xml = @"
                <groups>
                    <group
                        id=""gases""
                        name=""{20215,401}""
                        factoryname=""{20215,404}""
                        icon=""be_upgrade_resource""
                        tags=""tradable"" />
                </groups>
            ".ToXDocument();
        var exporter = new WareGroupExporter(new DummyCat(xml), new DummyLanguageResolver());

        Assert.Equal(exporter.GetRecordsAsync(pg, ct).ToEnumerable(), new[] { new WareGroup(
            WareGroupID: "gases",
            Name: "{20215,401}",
            Tier: 0
        )});
    }


    /// <summary>
    /// コンストラクタに渡された XML をマクロとして返すダミー CatFile クラス
    /// </summary>
    internal class DummyCat : ICatFile
    {
        /// <summary>
        /// OpenIndexXml で返す XML
        /// </summary>
        private readonly XDocument _Xml;


        /// <summary>
        /// 引数に渡された XML を返すだけのダミー CatFile クラスを初期化する
        /// </summary>
        /// <param name="xml"></param>
        internal DummyCat(XDocument xml) => _Xml = xml;


        public Task<Stream> OpenFileAsync(string filePath, CancellationToken cancellationToken)
            => throw new NotSupportedException();


        public Task<Stream?> TryOpenFileAsync(string filePath, CancellationToken cancellationToken)
            => Task.FromResult<Stream?>(null);


        /// <summary>
        /// インデックスの解決を行う代わりに、コンストラクタに渡された XML を返す
        /// </summary>
        /// <returns>コンストラクタで与えられた XML</returns>
        public Task<XDocument> OpenIndexXmlAsync(string indexFilePath, string name, CancellationToken cancellationToken = default) => Task.FromResult(_Xml);


        public Task<XDocument> OpenXmlAsync(string filePath, CancellationToken cancellationToken = default) => Task.FromResult(_Xml);


        public Task<XDocument?> TryOpenXmlAsync(string filePath, CancellationToken cancellationToken = default) => Task.FromResult<XDocument?>(_Xml);
    }


    /// <summary>
    /// 引数をそのまま返すダミー LanguageResolver クラス
    /// </summary>
    internal class DummyLanguageResolver : ILanguageResolver
    {
        /// <summary>
        /// 言語フィールド文字列を解決する代わりに、引数をそのまま帰す
        /// </summary>
        /// <param name="target">言語フィールド文字列を含む文字列</param>
        /// <returns>引数そのまま</returns>
        public string Resolve(string target) => target;
    }
}

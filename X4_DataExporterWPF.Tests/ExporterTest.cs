using LibX4.FileSystem;
using LibX4.Lang;
using System;
using System.IO;
using System.Xml.Linq;
using X4_DataExporterWPF.Entity;
using X4_DataExporterWPF.Export;
using Xunit;

namespace X4_DataExporterWPF.Tests
{
    public class ExporterTest
    {
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

            Assert.Equal(exporter.GetRecords(), new[] { new Module(
                moduleID: "module_arg_hab_l_01",
                moduleTypeID: "habitation",
                macro: "hab_arg_l_01_macro",
                maxWorkers: 0,
                workersCapacity: 1000,
                noBlueprint: false,
                thumbnail: null
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

            Assert.Equal(exporter.GetRecords(), new[] { new Module(
                moduleID: "module_arg_prod_foodrations_01",
                moduleTypeID: "production",
                macro: "prod_arg_foodrations_macro",
                maxWorkers: 90,
                workersCapacity: 0,
                noBlueprint: false,
                thumbnail: null
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
                wareID: "module_arg_conn_base_01",
                factionID: "antigone"
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

            Assert.Equal(exporter.GetRecords(), new[] { new WareEffect(
                wareID: "advancedcomposites",
                method: "default",
                effectID: "work",
                product: 0.0
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

            Assert.Equal(exporter.GetRecords(), new[] { new WareGroup(
                wareGroupID: "gases",
                name: "{20215,401}",
                tier: 0
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


            public MemoryStream OpenFile(string filePath)
                => throw new NotSupportedException();


            public MemoryStream? TryOpenFile(string filePath)
                => null;


            /// <summary>
            /// インデックスの解決を行う代わりに、コンストラクタに渡された XML を返す
            /// </summary>
            /// <returns>コンストラクタで与えられた XML</returns>
            public XDocument OpenIndexXml(string indexFilePath, string name) => _Xml;


            public XDocument OpenXml(string filePath) => _Xml;


            public XDocument? TryOpenXml(string filePath) => _Xml;
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
}

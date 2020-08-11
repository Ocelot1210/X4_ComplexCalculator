using System.Xml.Linq;
using LibX4.FileSystem;
using LibX4.Lang;
using X4_DataExporterWPF.Entity;
using X4_DataExporterWPF.Export;
using Xunit;

namespace X4_DataExporterWPF.Tests
{
    public class ExporterTest
    {
        /// <summary>
        /// 重複する EquipmentOwner は無視する
        /// 参照: <a href="https://github.com/Ocelot1210/X4_DataExporterWPF/pull/6">X4_DataExporterWPF#6</a>
        /// </summary>
        [Fact]
        public void EquipmentOwnerIgnoreDuplicates()
        {
            var xml = @"
                <wares>
                    <ware id=""engine_arg_l_allround_01_mk1"" transport=""equipment"">
                        <owner faction=""alliance"" />
                        <owner faction=""alliance"" />
                    </ware>
                </wares>
            ".ToXDocument();
            var exporter = new EquipmentOwnerExporter(xml);

            Assert.Equal(exporter.GetRecords(), new[] { new EquipmentOwner(
                equipmentID: "engine_arg_l_allround_01_mk1",
                factionID: "alliance"
            )});
        }


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
            var exporter = new ModuleExporter(new DummyCat(macroXml), wareXml, new DummyLanguageResolver());

            Assert.Equal(exporter.GetRecords(), new[] { new Module(
                moduleID: "module_arg_hab_l_01",
                moduleTypeID: "habitation",
                name: "{20104,30301}",
                macro: "hab_arg_l_01_macro",
                maxWorkers: 0,
                workersCapacity: 1000,
                noBluePrint: 0
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
            var exporter = new ModuleExporter(new DummyCat(macroXml), wareXml, new DummyLanguageResolver());

            Assert.Equal(exporter.GetRecords(), new[] { new Module(
                moduleID: "module_arg_prod_foodrations_01",
                moduleTypeID: "production",
                name: "{20104,13401}",
                macro: "prod_arg_foodrations_macro",
                maxWorkers: 90,
                workersCapacity: 0,
                noBluePrint: 0
            )});
        }


        /// <summary>
        /// 重複する ModuleOwner は無視する
        /// 参照: <a href="https://github.com/Ocelot1210/X4_ComplexCalculator/pull/6">#6</a>
        /// </summary>
        [Fact]
        public void ModuleOwnerIgnoreDuplicates()
        {
            var xml = @"
                <wares>
                    <ware id=""module_arg_conn_base_01"" tags=""module"">
                        <owner faction=""antigone"" />
                        <owner faction=""antigone"" />
                    </ware>
                </wares>
            ".ToXDocument();
            var exporter = new ModuleOwnerExporter(xml);

            Assert.Equal(exporter.GetRecords(), new[] { new ModuleOwner(
                moduleID: "module_arg_conn_base_01",
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
            var exporter = new WareGroupExporter(xml, new DummyLanguageResolver());

            Assert.Equal(exporter.GetRecords(), new[] { new WareGroup(
                wareGroupID: "gases",
                name: "{20215,401}",
                factoryName: "{20215,404}",
                icon: "be_upgrade_resource",
                tier: 0
            )});
        }


        /// <summary>
        /// コンストラクタに渡された XML をマクロとして返すダミー CatFile クラス
        /// </summary>
        internal class DummyCat : IIndexResolver
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

            /// <summary>
            /// インデックスの解決を行う代わりに、コンストラクタに渡された XML を返す
            /// </summary>
            /// <returns>コンストラクタで与えられた XML</returns>
            public XDocument OpenIndexXml(string indexFilePath, string name) => _Xml;
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

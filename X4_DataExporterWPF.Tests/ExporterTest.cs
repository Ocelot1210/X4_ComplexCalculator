using X4_DataExporterWPF.Entity;
using X4_DataExporterWPF.Export;
using Xunit;

namespace X4_DataExporterWPF.Tests
{
    public class ExporterTest
    {
        /// <summary>
        /// 重複する EquipmentOwner は無視する。
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
        /// 重複する ModuleOwner は無視する。
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
    }
}

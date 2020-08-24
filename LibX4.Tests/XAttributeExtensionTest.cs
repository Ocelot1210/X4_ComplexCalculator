using System.Globalization;
using System.Threading;
using System.Xml.Linq;
using LibX4.Xml;
using Xunit;

namespace LibX4.Tests
{
    /// <summary>
    /// <see cref="XAttributeExtension"/> のテストクラス
    /// </summary>
    public class XAttributeExtensionTest
    {
        /// <summary>
        /// 言語設定によっては double のパースに失敗する場合がある
        /// 参照: <a href="https://github.com/Ocelot1210/X4_ComplexCalculator/issues/5">#5</a>
        /// </summary>
        [Fact]
        public void ParseFailedInSomeCurrentCulture()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("fr-FR");

            var time = new XAttribute("time", "1.5");
            Assert.Equal(1.5, time.GetDouble());
        }
    }
}

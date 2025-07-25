using LibX4.Xml;
using System.Globalization;
using System.Threading;
using System.Xml.Linq;
using Xunit;

namespace LibX4.Tests;

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


    /// <summary>
    /// 整数を期待する属性に実数が混じるとデータ抽出に失敗する
    /// 参照: <a href="https://github.com/Ocelot1210/X4_ComplexCalculator/issues/190>#190</a>
    /// </summary>
    [Fact]
    public void ParseDecimalInt()
    {
        var price = new XAttribute("min", "87682.6");
        Assert.Equal(87682, price.GetInt());
    }
}

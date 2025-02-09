using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Globalization;

namespace X4_ComplexCalculator.Main.Menu.Lang
{
    /// <summary>
    /// 言語が変わった事を表すメッセージ
    /// </summary>
    /// <param name="cultureInfo">変更後の言語</param>
    public class LangChangedMessage(CultureInfo cultureInfo) : ValueChangedMessage<CultureInfo>(cultureInfo)
    {
    }
}

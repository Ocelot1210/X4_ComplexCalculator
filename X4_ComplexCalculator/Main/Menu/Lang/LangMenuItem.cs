using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Globalization;
using System.Linq;
using WPFLocalizeExtension.Engine;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.LocalizationProviders;

namespace X4_ComplexCalculator.Main.Menu.Lang;

/// <summary>
/// 言語メニュー1レコード分
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="cultureInfo">言語情報</param>
public partial class LangMenuItem(IMessenger messenger, CultureInfo cultureInfo, bool isChecked) : ObservableRecipient(messenger), IRecipient<LangChangedMessage>
{
    #region メンバ
    /// <summary>
    /// 言語
    /// </summary>
    private readonly CultureInfo _cultureInfo = cultureInfo;
    #endregion


    #region プロパティ
    /// <summary>
    /// 言語名
    /// </summary>
    public string Name => _cultureInfo.NativeName;


    /// <summary>
    /// チェックされたか
    /// </summary>
    [ObservableProperty]
    public partial bool IsChecked { get; set; } = isChecked;


    /// <summary>
    /// チェックを外すことはできない
    /// </summary>
    [ObservableProperty]
    public partial bool IsCheckable { get; set; } = !isChecked;

    #endregion


    /// <summary>
    /// 選択言語変更中
    /// </summary>
    /// <param name="value"></param>
    [RelayCommand]
    partial void OnIsCheckedChanging(bool value)
    {
        // 言語が選択されたら他のアイテムにメッセージを通知
        if (value)
        {
            Messenger.Send(new LangChangedMessage(_cultureInfo));
            LocalizeDictionary.Instance.Culture = _cultureInfo;
            Configuration.Instance.Language = _cultureInfo;
        }
    }


    /// <summary>
    /// 言語変更メッセージ受信時
    /// </summary>
    /// <param name="message"></param>
    public void Receive(LangChangedMessage message)
    {
        if (message.Value != _cultureInfo)
        {
            // 別の言語が選択されたらチェックを外してチェック可能にする
            IsChecked = false;
            IsCheckable = true;
        }
        else
        {
            // 言語が同じならチェックを外させないようにする
            IsCheckable = false;
        }
    }


    /// <summary>
    /// 言語一覧を作成
    /// </summary>
    /// <returns></returns>
    public static LangMenuItem[] CreateItems()
    {
        if (LocalizeDictionary.Instance.DefaultProvider is CSVLocalizationProvider provider)
        {
            provider.FileName = "Lang";
        }

        LocalizeDictionary.Instance.Culture = Configuration.Instance.Language;

        var messenger = new WeakReferenceMessenger();

        var ret = LocalizeDictionary.Instance.DefaultProvider.AvailableCultures
            .Where(x => !string.IsNullOrEmpty(x.Name))
            .Select(x => new LangMenuItem(messenger, x, LocalizeDictionary.Instance.Culture.Name == x.Name))
            .ToArray();

        foreach (var item in ret) 
        {
            messenger.Register(item);
        }

        return ret;
    }
}

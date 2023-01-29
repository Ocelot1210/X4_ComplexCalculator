using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using WPFLocalizeExtension.Engine;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.Main.Menu.Lang;

namespace X4_ComplexCalculator.Main;

/// <summary>
/// 言語一覧管理用クラス
/// </summary>
internal class LanguagesManager : IDisposable
{
    #region フィールド
    /// <summary>
    /// 購読解除用
    /// </summary>
    private readonly IDisposable _disposables;
    #endregion


    #region プロパティ
    /// <summary>
    /// 言語一覧
    /// </summary>
    public IReadOnlyList<LangMenuItem> Languages { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public LanguagesManager()
    {
        if (LocalizeDictionary.Instance.DefaultProvider is CSVLocalizationProvider provider)
        {
            provider.FileName = "Lang";
        }

        LocalizeDictionary.Instance.Culture = Configuration.Instance.Language;

        Languages = LocalizeDictionary.Instance.DefaultProvider.AvailableCultures
            .Where(x => !string.IsNullOrEmpty(x.Name))
            .Select(x => new LangMenuItem(x, LocalizeDictionary.Instance.Culture.Name == x.Name))
            .ToArray();

        // 他の言語が選択された時、設定言語の変更を実行
        _disposables = Languages
            .Select(x => x.IsChecked.Where(v => v).Select(_ => x.CultureInfo))
            .Merge()
            .Subscribe(ApplyLanguageChange);
    }


    /// <summary>
    /// 設定言語の変更
    /// </summary>
    /// <param name="cultureInfo">変更後の言語</param>
    private void ApplyLanguageChange(CultureInfo cultureInfo)
    {
        LocalizeDictionary.Instance.Culture = cultureInfo;
        Configuration.Instance.Language = cultureInfo;

        foreach (var lang in Languages.Where(x => x.CultureInfo != cultureInfo))
        {
            lang.IsChecked.Value = false;
        }
    }


    /// <inheritdoc />
    public void Dispose() => _disposables.Dispose();
}

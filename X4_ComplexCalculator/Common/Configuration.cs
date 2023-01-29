using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace X4_ComplexCalculator.Common;

/// <summary>
/// 設定ファイル読み込み用
/// </summary>
public class Configuration
{
    #region スタティックプロパティ
    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static Configuration Instance { get; } = new("App.config.json");
    #endregion


    #region メンバ
    /// <summary>
    /// 設定ファイルオブジェクト
    /// </summary>
    private readonly IConfigurationRoot _config;
    #endregion


    #region プロパティ
    /// <summary>
    /// X4DBのファイルパス
    /// </summary>
    public string X4DBPath => _config["AppSettings:X4DBPath"] ?? 
        throw new InvalidDataException((string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("Lang:System_InvalidConfigFile", null, null));


    /// <summary>
    /// CommonDBのファイルパス
    /// </summary>
    public string CommonDBPath => _config["AppSettings:CommonDBPath"] ?? 
        throw new InvalidDataException((string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("Lang:System_InvalidConfigFile", null, null));



    /// <summary>
    /// 言語
    /// </summary>
    public CultureInfo Language
    {
        get
        {
            var lang = _config["AppSettings:Language"];

            try
            {
                // 言語が設定されているか？
                if (!string.IsNullOrWhiteSpace(lang))
                {
                    // 言語が設定されていればそれを使用
                    return CultureInfo.GetCultureInfo(lang);
                }
                else
                {
                    // 言語が設定されていない場合、システムのロケールを設定
                    return CultureInfo.CurrentUICulture;
                }
            }
            catch (CultureNotFoundException)
            {
                // 無効な言語が指定されている場合はシステムのロケールを設定
                return CultureInfo.CurrentUICulture;
            }
        }
        set
        {
            SetValue("AppSettings.Language", value.Name);
        }
    }


    /// <summary>
    /// 起動時に更新を確認するか
    /// </summary>
    public bool CheckUpdateAtLaunch
    {
        get => _config["AppSettings:CheckUpdateAtLaunch"] != bool.FalseString;
        set => SetValue("AppSettings.CheckUpdateAtLaunch", value.ToString());
    }
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="path">設定ファイルのパス</param>
    private Configuration(string path)
    {
        // 設定ファイルが無効なら修復する
        if (!IsValidSettingFile(path))
        {
            FixSettingFile(path);
        }

        _config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile(path)
            .Build();
    }


    /// <summary>
    /// 設定ファイルが有効か判定する
    /// </summary>
    /// <param name="path">判定対象のファイルパス</param>
    /// <returns></returns>
    private static bool IsValidSettingFile(string path)
    {
        var ret = true;

        try
        {
            var conf = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(path)
                .Build();

            // 値が入っているかチェック
            string[] keys =
            {
                "AppSettings:X4DBPath",
                "AppSettings:CommonDBPath",
                "AppSettings:Language",
                "AppSettings:CheckUpdateAtLaunch"
            };

            ret = keys.All(x => conf[x] is not null);
        }
        catch
        {
            ret = false;
        }

        return ret;
    }


    /// <summary>
    /// 設定ファイルを修復する
    /// </summary>
    /// <param name="path">修復対象の設定ファイルパス</param>
    private static void FixSettingFile(string path)
    {
        IConfigurationRoot? conf = null;

        try
        {
            conf = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(path)
                .Build();
        }
        catch
        {

        }


        var json = new JObject(
            new JProperty(
                "AppSettings",
                new JObject(
                    new JProperty("X4DBPath", conf?["AppSettings:X4DBPath"] ?? ".\\db\\x4.db"),
                    new JProperty("CommonDBPath", conf?["AppSettings:CommonDBPath"] ?? ".\\db\\common.db"),
                    new JProperty("Language", conf?["AppSettings:Language"] ?? ""),
                    new JProperty("CheckUpdateAtLaunch", conf?["AppSettings:Language"] ?? "True")
                )));

        var text = JsonConvert.SerializeObject(json, Formatting.Indented);
        File.WriteAllText(path, text);
    }



    /// <summary>
    /// App.config.jsonに値を設定する
    /// </summary>
    /// <param name="key">キー</param>
    /// <param name="value">設定値</param>
    private void SetValue(string key, string value)
    {
        var provider = _config.Providers.OfType<JsonConfigurationProvider>().First();
        if (provider.Source.FileProvider is null) return;
        if (provider.Source.Path is null) return;

        var path = provider.Source.FileProvider.GetFileInfo(provider.Source.Path).PhysicalPath;
        if (path is null) return;

        var jsonText = File.ReadAllText(path);
        var jsonObj = (JObject?)JsonConvert.DeserializeObject(jsonText) ?? throw new InvalidOperationException();
        

        var token = jsonObj.SelectToken(key) ?? throw new InvalidOperationException();
        if (token is JValue jValue)
        {
            jValue.Value = value;

            string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText(path, output);

            _config.Reload();
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}

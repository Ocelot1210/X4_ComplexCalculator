using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace X4_ComplexCalculator.Common
{
    /// <summary>
    /// 設定ファイル読み込み用
    /// </summary>
    class Configuration
    {
        private static IConfigurationRoot? Config;


        /// <summary>
        /// 設定ファイルオブジェクトを取得する
        /// </summary>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration()
        {
            if (Config == null)
            {
                Config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("App.config.json")
                    .Build();
            }

            return Config;
        }


        /// <summary>
        /// App.config.jsonに値を設定する
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">設定値</param>
        public static void SetValue(string key, string value)
        {
            if (Config == null)
            {
                throw new InvalidOperationException();
            }

            var provider = Config.Providers.OfType<JsonConfigurationProvider>().First();

            var path = provider.Source.FileProvider.GetFileInfo(provider.Source.Path).PhysicalPath;

            var jsonText = File.ReadAllText(path);
            var jsonObj = (JObject?)JsonConvert.DeserializeObject(jsonText) ?? throw new InvalidOperationException();
            var token = jsonObj.SelectToken(key) ?? throw new InvalidOperationException();
            if (token is JValue jValue)
            {
                jValue.Value = value;

                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(path, output);

                Config.Reload();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}

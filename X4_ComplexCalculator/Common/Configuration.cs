using Microsoft.Extensions.Configuration;
using System.IO;

namespace X4_ComplexCalculator.Common
{
    /// <summary>
    /// 設定ファイル読み込み用
    /// </summary>
    class Configuration
    {
        private static IConfigurationRoot Config;


        /// <summary>
        /// 設定ファイルオブジェクトを取得する
        /// </summary>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration()
        {
            if (Config == null)
            {
                Config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("App.config.json")
                    .Build();
            }

            return Config;
        }
    }
}

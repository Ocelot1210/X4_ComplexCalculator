using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// ユーティリティクラス
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// tagsの中からサイズIDを取得する
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static string GetSizeIDFromTags(string? tags)
        {
            if (string.IsNullOrEmpty(tags))
            {
                return "";
            }

            string[] size =
            {
                "extrasmall",
                "small",
                "medium",
                "large",
                "extralarge"
            };

            return SplitTags(tags).FirstOrDefault(x => size.Contains(x));
        }


        /// <summary>
        /// tagsを分割する
        /// </summary>
        /// <param name="tags"></param>
        /// <returns>分割結果</returns>
        public static string[] SplitTags(string? tags)
        {
            if (string.IsNullOrEmpty(tags))
            {
                return Array.Empty<string>();
            }

            // たまにtagsが[]で囲われてる場合があるため[]を削除してからSplitする
            return tags.TrimStart('[').TrimEnd(']').Split(' ');
        }
    }
}

using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using LibX4.FileSystem;

namespace LibX4.Lang
{
    /// <summary>
    /// X4の言語フィールド文字列を解決するクラス
    /// </summary>
    public class LanguageResolver
    {
        #region メンバ
        /// <summary>
        /// 言語ファイルのスタック
        /// </summary>
        private readonly XDocument[] _LanguagesXml;


        /// <summary>
        /// メッセージテンプレートからIDを抽出する正規表現
        /// </summary>
        private static readonly Regex _GetIDRegex = new Regex(@"\{\s*(\d+)\s*,\s*(\d+)\s*\}");


        /// <summary>
        /// コメント削除用正規表現
        /// </summary>
        private static readonly Regex _RemoveCommentRegex = new Regex(@"(?<!\\)\(.*?(?<!\\)\)");


        /// <summary>
        /// 括弧のエスケープを解除する正規表現
        /// </summary>
        private static readonly Regex _UnescapeRegex = new Regex(@"\\(.)");
        #endregion


        /// <summary>
        /// 指定された言語 XML で LanguageResolver インスタンスを初期化する。
        /// 複数の言語 XML が指定された場合、先に指定された物を優先する。
        /// </summary>
        /// <param name="languageXml">参照する言語 XML</param>
        internal LanguageResolver(params XDocument[] languageXml) => _LanguagesXml = languageXml;


        /// <summary>
        /// 指定された言語 ID のファイルを読み込み、LanguageResolver インスタンスを初期化する。
        /// 複数の言語 ID が指定された場合、先に指定された物を優先する。
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        /// <param name="languageIds">読み込む言語 ID の配列</param>
        public LanguageResolver(CatFile catFile, params int[] languageIds)
        {
            _LanguagesXml = languageIds
                .Where((x, i) => languageIds.Take(i).All(y => x != y))  // 順序を保証する Distinct
                .Select(languageId => $"t/0001-l{languageId,3:D3}.xml")
                .Append("t/0001.xml")
                .Select(languageFilePath => catFile.OpenXml(languageFilePath))
                .ToArray();
        }


        /// <summary>
        /// 言語フィールドの文字列を言語ファイルを参照して解決する
        /// </summary>
        /// <param name="template">言語フィールド文字列</param>
        /// <returns>解決結果文字列</returns>
        public string Resolve(string template)
        {
            if (string.IsNullOrEmpty(template))
            {
                return template;
            }

            foreach (var langTree in _LanguagesXml)
            {
                var textOld = "";
                var textNew = template;
                var succeeded = false;

                while (textOld != textNew)
                {
                    textOld = textNew;
                    bool succededTmp;

                    (textNew, succededTmp) = ResolveField(textNew, langTree);

                    succeeded |= succededTmp;
                }

                // 名前解決に成功したか？
                if (succeeded)
                {
                    return _UnescapeRegex.Replace(textNew, @"$1");
                }
            }

            return template;
        }


        /// <summary>
        /// 言語フィールド解決処理メイン
        /// </summary>
        /// <param name="text">言語フィールド文字列</param>
        /// <param name="langTree">翻訳対象言語のXDocument</param>
        /// <returns></returns>
        private (string, bool) ResolveField(string text, XDocument langTree)
        {
            var match = _GetIDRegex.Match(text);

            var pageID = match.Groups[1].Value;
            var tID = match.Groups[2].Value;

            var nodes = langTree.Root.XPathSelectElements($"./page[@id='{pageID}']/t[@id='{tID}']");
            if (nodes == null || !nodes.Any())
            {
                return (text, false);
            }

            // コメントアウト削除
            var ret = _RemoveCommentRegex.Replace(nodes.First().Value, "");
            return (text.Replace(match.Value, ret), true);
        }
    }
}

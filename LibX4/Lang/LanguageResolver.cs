using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using LibX4.FileSystem;

namespace LibX4.Lang
{
    /// <summary>
    /// X4 の言語フィールド文字列 (例: {1001,2490}) を解決するクラス
    /// </summary>
    public class LanguageResolver : ILanguageResolver
    {
        #region メンバ
        /// <summary>
        /// 読み込んだ言語 XML
        /// </summary>
        private readonly XDocument[] _LanguagesXml;


        /// <summary>
        /// 言語フィールド文字列から pageID, tID を抽出する正規表現
        /// </summary>
        private static readonly Regex _GetIDRegex = new(@"\{\s*(\d+)\s*,\s*(\d+)\s*\}");


        /// <summary>
        /// エスケープされていない括弧とその内部を削除する正規表現
        /// </summary>
        private static readonly Regex _RemoveCommentRegex = new(@"(?<!\\)\((?:|.*[^\\])\)");


        /// <summary>
        /// エスケープを解除する正規表現
        /// </summary>
        private static readonly Regex _UnescapeRegex = new(@"\\(.)");
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
            var languageXmls = languageIds
                .Where((x, i) => languageIds.Take(i).All(y => x != y))  // 順序を保証する Distinct
                .Select(languageId => catFile.OpenXml($"t/0001-l{languageId,3:D3}.xml"));

            var defaultLanguageXml = catFile.TryOpenXml("t/0001.xml");
            if (defaultLanguageXml is not null) languageXmls = languageXmls.Append(defaultLanguageXml);

            _LanguagesXml = languageXmls.ToArray();
        }


        /// <summary>
        /// 言語フィールド文字列 (例: {1001,2490}) を解決する
        /// </summary>
        /// <param name="target">言語フィールド文字列を含む文字列</param>
        /// <returns>言語フィールド文字列を解決し置き換えた文字列</returns>
        public string Resolve(string target)
        {
            if (string.IsNullOrEmpty(target)) return target;

            var matchLangField = _GetIDRegex.Match(target);
            if (matchLangField == null) return target;
            var pageID = matchLangField.Groups[1].Value;
            var tID = matchLangField.Groups[2].Value;

            foreach (var languageXml in _LanguagesXml)
            {
                var findT = languageXml.Root
                    ?.XPathSelectElement($"./page[@id='{pageID}']/t[@id='{tID}']")
                    ?.Value;
                if (findT is not null)
                {
                    findT = findT.Replace("\\n", "\n");
                    var uncommentedT = _RemoveCommentRegex.Replace(findT, "");
                    var resolvedText = target.Replace(matchLangField.Value, uncommentedT);
                    var unescapedText = _UnescapeRegex.Replace(resolvedText, "$1");
                    return Resolve(unescapedText);
                }
            }

            return target;
        }
    }
}

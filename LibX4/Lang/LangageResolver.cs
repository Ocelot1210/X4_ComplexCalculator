using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using LibX4.FileSystem;

namespace LibX4.Lang
{
    public class LangageResolver
    {
        /// <summary>
        /// 言語ファイル管理用辞書[Key = 言語ファイルパス, Value = 言語xml]
        /// </summary>
        private Dictionary<string, XDocument> _LangTrees = new Dictionary<string, XDocument>();


        /// <summary>
        /// 読み込んだ言語一覧(最後の要素が優先)
        /// </summary>
        private Stack<string> _Langages = new Stack<string>();


        /// <summary>
        /// catファイルオブジェクト
        /// </summary>
        private readonly CatFile _CatFile;


        /// <summary>
        /// メッセージテンプレートからIDを抽出する正規表現
        /// </summary>
        private readonly Regex _GetIDRegex = new Regex(@"\{\s*(\d+)\s*,\s*(\d+)\s*\}");


        /// <summary>
        /// コメント削除用正規表現
        /// </summary>
        private readonly Regex _RemoveCommentRegex = new Regex(@"(?<!\\)\(.*?(?<!\\)\)");

        /// <summary>
        /// 括弧のエスケープを解除する正規表現
        /// </summary>
        private readonly Regex _UnescapeRegex = new Regex(@"\\(.)");


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        public LangageResolver(CatFile catFile)
        {
            _CatFile = catFile;
        }


        /// <summary>
        /// 言語ファイル読み込み
        /// </summary>
        /// <param name="langID">言語ID</param>
        public void LoadLangFile(int langID)
        {
            var langFilePath = $"t/0001-l{langID,3:D3}.xml";

            // ロード済みなら何もしない
            if (_LangTrees.ContainsKey(langFilePath))
            {
                return;
            }

            _LangTrees.Add(langFilePath, _CatFile.OpenLangXml(langFilePath));
            _Langages.Push(langFilePath);
        }


        /// <summary>
        /// 言語フィールドの文字列を言語ファイルを参照して解決する
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public string Resolve(string template)
        {
            if (string.IsNullOrEmpty(template))
            {
                return template;
            }


            foreach (var langTree in _Langages.Select(x => _LangTrees[x]))
            {
                var textOld = "";
                var textNew = template;
                var succeeded = false;

                while (textOld != textNew)
                {
                    textOld = textNew;
                    var succededTmp = false;

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
        /// <param name="langTree"></param>
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

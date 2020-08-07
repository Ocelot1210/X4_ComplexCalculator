﻿using System.Collections.Generic;
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
        private readonly Stack<XDocument> _LanguagesXml = new Stack<XDocument>();


        /// <summary>
        /// 読み込み済みの言語ファイル名のコレクション
        /// </summary>
        private readonly HashSet<string> _LoadedLanguages = new HashSet<string>();


        /// <summary>
        /// catファイルオブジェクト
        /// </summary>
        private readonly CatFile _CatFile;


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
        /// コンストラクタ
        /// </summary>
        /// <param name="catFile">catファイルオブジェクト</param>
        public LanguageResolver(CatFile catFile)
        {
            _CatFile = catFile;
        }


        /// <summary>
        /// 言語ファイル読み込み
        /// </summary>
        /// <param name="langID">言語ID</param>
        public void LoadLangFile(int langID)
        {
            LoadLangFile($"t/0001-l{langID,3:D3}.xml");
        }


        /// <summary>
        /// 言語ファイル読み込み
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadLangFile(string fileName)
        {
            // ロード済みなら何もしない
            if (_LoadedLanguages.Contains(fileName))
            {
                return;
            }

            _LanguagesXml.Push(_CatFile.OpenLangXml(fileName));
            _LoadedLanguages.Add(fileName);
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

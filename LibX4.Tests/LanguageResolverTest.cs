﻿using LibX4.Lang;
using Xunit;

namespace LibX4.Tests
{
    /// <summary>
    /// <see cref="LanguageResolver"/> のテストクラス
    /// </summary>
    public class LanguageResolverTest
    {
        /// <summary>
        /// 最初に指定された XML を優先する
        /// </summary>
        [Fact]
        public void PreferFirstSpecifiedXml()
        {
            var langXml1st = @"
            <language>
                <page id=""1001"">
                     <t id=""1"">船体</t>
                </page>
            </language>
            ".ToXDocument();
            var langXml2nd = @"
            <language>
                <page id=""1001"">
                     <t id=""1"">Hull</t>
                </page>
            </language>
            ".ToXDocument();
            var resolve = new LanguageResolver(langXml1st, langXml2nd).Resolve("{1001,1}");
            Assert.Equal("船体", resolve);
        }


        /// <summary>
        /// 最初に指定された XML に無かった場合、次の XML を参照する
        /// </summary>
        [Fact]
        public void ReadNextXmlIfNotFound()
        {
            var langXml1st = "<language></language>".ToXDocument();
            var langXml2st = @"
            <language>
                <page id=""1001"">
                     <t id=""1"">Hull</t>
                </page>
            </language>
            ".ToXDocument();
            var resolve = new LanguageResolver(langXml1st, langXml2st).Resolve("{1001,1}");
            Assert.Equal("Hull", resolve);
        }


        /// <summary>
        /// 解決後の文字列に言語フィールド文字列が含まれる場合、再帰的に処理する
        /// </summary>
        [Fact]
        public void Recursion()
        {
            var resolve = new LanguageResolver(@"
            <language>
                <page id=""1001"">
                     <t id=""5802"">Planned Amount of {20201, 1501}</t>
                </page>
                <page id=""20201"">
                    <t id=""1501"">Graphene</t>
                </page>
            </language>
            ".ToXDocument()).Resolve("{1001,5802}");
            Assert.Equal("Planned Amount of Graphene", resolve);
        }


        /// <summary>
        /// 括弧で囲われた文字列はコメントとして扱い、削除する
        /// </summary>
        [Fact]
        public void BranketsIsComment()
        {
            var resolve = new LanguageResolver(@"
            <language>
                <page id=""1001"">
                     <t id=""9"">(Storage)None</t>
                </page>
            </language>
            ".ToXDocument()).Resolve("{1001,9}");
            Assert.Equal("None", resolve);
        }


        /// <summary>
        /// 括弧の前にバックスペースがある場合、
        /// バックスペースを除去した上で括弧を通常の文字として扱う。
        /// </summary>
        [Fact]
        public void BranketsEscape()
        {
            var resolve = new LanguageResolver(@"
            <language>
                <page id=""1001"">
                    <t id=""2490"">Shield Generators \(including groups\)</t>
                </page>
            </language>
            ".ToXDocument()).Resolve("{1001,2490}");
            Assert.Equal("Shield Generators (including groups)", resolve);
        }
    }
}

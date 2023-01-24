using LibX4.Lang;
using Xunit;

namespace LibX4.Tests.LanguageResolverTest;

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
    /// 解決後の文字列に言語フィールド文字列が含まれる場合、再帰的に解決する
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
    /// 解決後の文字列に言語フィールド文字列が含まれる場合、含まれなくなるまで再帰的に解決する
    /// </summary>
    [Fact]
    public void MultipleRecursion()
    {
        var resolve = new LanguageResolver(@"
            <language>
                <page id=""1001"">
                     <t id=""5802"">Planned Amount of {20201, 1501}</t>
                </page>
                <page id=""20201"">
                    <t id=""1501"">{20201, 1502}</t>
                    <t id=""1502"">Graphene</t>
                </page>
            </language>
            ".ToXDocument()).Resolve("{1001,5802}");
        Assert.Equal("Planned Amount of Graphene", resolve);
    }


    /// <summary>
    /// 解決後の文字列に言語フィールド文字列が 2 つ以上含まれる場合、全て解決する
    /// </summary>
    [Fact]
    public void MultipleFieldResolve()
    {
        var resolve = new LanguageResolver(@"
            <language>
                <page id=""20101"">
                    <t id=""10101"">Discoverer</t>
                    <t id=""10102"">(Discoverer Vanguard){20101,10101} {20111,1101}</t>
                </page>
                <page id=""20111"">
                    <t id=""1101"">Vanguard</t>
                </page>
            </language>
            ".ToXDocument()).Resolve("{20101,10102}");
        Assert.Equal("Discoverer Vanguard", resolve);
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


    /// <summary>
    /// コメントと括弧が重なっている場合の解決 1
    /// </summary>
    [Fact]
    public void MixingCommentsAndBrankets_1()
    {
        var resolve = new LanguageResolver(@"
            <language>
                <page id=""1"">
                    <t id=""1"">aaaa\(bbbb(cccc)\)</t>
                </page>
            </language>
            ".ToXDocument()).Resolve("{1,1}");
        Assert.Equal("aaaa(bbbb)", resolve);
    }


    /// <summary>
    /// コメントと括弧が重なっている場合の解決 2
    /// </summary>
    [Fact]
    public void MixingCommentsAndBrankets_2()
    {
        var resolve = new LanguageResolver(@"
            <language>
                <page id=""1"">
                    <t id=""1"">aaaa\(bbbb(cc\)cc)</t>
                </page>
            </language>
            ".ToXDocument()).Resolve("{1,1}");
        Assert.Equal("aaaa(bbbb", resolve);
    }


    /// <summary>
    /// コメントと括弧が重なっている場合の解決 3
    /// </summary>
    [Fact]
    public void MixingCommentsAndBrankets_3()
    {
        var resolve = new LanguageResolver(@"
            <language>
                <page id=""1"">
                    <t id=""1"">aaaa(\(bbbb)cc\)cc</t>
                </page>
            </language>
            ".ToXDocument()).Resolve("{1,1}");
        Assert.Equal("aaaacc)cc", resolve);
    }


    /// <summary>
    /// コメントと括弧が重なっている場合の解決 4
    /// </summary>
    [Fact]
    public void MixingCommentsAndBrankets_4()
    {
        var resolve = new LanguageResolver(@"
            <language>
                <page id=""1"">
                    <t id=""1"">aaaa(\(bbbb\)cc)cc</t>
                </page>
            </language>
            ".ToXDocument()).Resolve("{1,1}");
        Assert.Equal("aaaacc", resolve);
    }



    /// <summary>
    /// 改行
    /// </summary>
    [Fact]
    public void NewLine()
    {
        var resolve = new LanguageResolver(@"
            <language>
                <page id=""1"">
                    <t id=""1"">aaaa\nbbbb</t>
                </page>
            </language>
            ".ToXDocument()).Resolve("{1,1}");
        Assert.Equal("aaaa\nbbbb", resolve);
    }



    /// <summary>
    /// ページ ID 省略時
    /// </summary>
    [Fact]
    public void OmitPageID()
    {
        var resolve = new LanguageResolver(@"
            <language>
                <page id=""1"">
                    <t id=""1"">aaaa</t>
                    <t id=""2"">{, 1}bbbb</t>
                </page>
            </language>
            ".ToXDocument());
        Assert.Equal("aaaabbbb", resolve.Resolve("{1,2}"));
    }
}

using System;
using System.Collections;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace LibX4.FileSystem;

/// <summary>
/// XML差分パッチ用クラス
/// </summary>
/// <remarks>
/// 詳細な仕様は RFC5261 を参照。
/// なお、現時点では名前空間のマングリングを伴うパッチ(RFC5261 の A.18)以外は
/// 期待する内容と同じ意味になるxmlが作成される。
/// </remarks>
static class XMLPatcher
{
    /// <summary>
    /// Xml ファイルに別の Xml ファイルをマージする
    /// </summary>
    /// <param name="baseXml">ベースとなる XML</param>
    /// <param name="patchXml">マージする XML</param>
    public static void MergeXML(this XDocument baseXml, XDocument patchXml)
    {
        ArgumentNullException.ThrowIfNull(baseXml.Root);
        ArgumentNullException.ThrowIfNull(patchXml.Root);

        if (patchXml.Root.Name.LocalName == "diff") ApplyDiffXml(baseXml, patchXml);
        else baseXml.Root.Add(patchXml.Root.Elements());
    }


    /// <summary>
    /// Xml ファイルに RFC5261 に準拠した diff XML をマージする
    /// </summary>
    /// <param name="baseXml">ベースとなる XML</param>
    /// <param name="diffXml">RFC5261 に準拠した diff XML</param>
    private static void ApplyDiffXml(XDocument baseXml, XDocument diffXml)
    {
        ArgumentNullException.ThrowIfNull(diffXml.Root);

        var nsMng = new XmlNamespaceManager(new NameTable());

        foreach (var attr in diffXml.Root.Attributes())
        {
            // 名前空間の場合
            if (attr.IsNamespaceDeclaration)
            {
                if (attr.Name.LocalName.StartsWith("xmlns"))
                {
                    nsMng.AddNamespace(attr.Name.NamespaceName, attr.Value);
                }
                else
                {
                    nsMng.AddNamespace(attr.Name.LocalName, attr.Value);
                }
            }
        }

        foreach (var elm in diffXml.Root.Elements())
        {
            var selectorText = elm.Attribute("sel")?.Value;
            if (selectorText is null) continue;

            var sel = baseXml.XPathEvaluate(selectorText, nsMng);

            if (sel is not IEnumerable enumerable)
            {
                continue;
            }

            var targetNode = enumerable.OfType<XObject>().FirstOrDefault();
            if (targetNode is null)
            {
                continue;
            }

            switch (elm.Name.LocalName)
            {
                case "add":
                    AddNode(elm, targetNode);
                    break;

                case "replace":
                    Replace(elm, targetNode);
                    break;

                case "remove":
                    Remove(baseXml, targetNode);
                    break;

                default:
                    break;
            }
        }
    }


    /// <summary>
    /// 要素を追加
    /// </summary>
    /// <param name="element"></param>
    /// <param name="target"></param>
    private static void AddNode(XElement element, XObject target)
    {
        var type = element.Attribute("type")?.Value;


        //////////////////////
        // ノード追加の場合 //
        //////////////////////
        if (string.IsNullOrEmpty(type))
        {
            switch (element.Attribute("pos")?.Value ?? "prepend")
            {
                case "after":
                    (target as XNode)?.AddAfterSelf(element.Nodes());
                    break;

                case "before":
                    (target as XNode)?.AddBeforeSelf(element.Nodes());
                    break;

                default:
                    (target as XElement)?.Add(element.Nodes());
                    break;
            }
            return;
        }

        ////////////////////
        // 属性追加の場合 //
        ////////////////////
        if (type.StartsWith('@'))
        {
            // 属性追加の場合
            (target as XElement)?.SetAttributeValue(type.Substring(1), element.Value);
            return;
        }


        ////////////////////////
        // 名前空間追加の場合 //
        ////////////////////////
        if (type.StartsWith("namespace::"))
        {
            var attr = new XAttribute(XNamespace.Xmlns + $"{type.Substring("namespace::".Length)}", element.Value);
            (target as XElement)?.Add(attr);
            return;
        }

        throw new NotImplementedException();
    }


    /// <summary>
    /// 要素を置換
    /// </summary>
    /// <param name="element"></param>
    /// <param name="target"></param>
    private static void Replace(XElement element, XObject target)
    {
        switch (target.NodeType)
        {
            // 属性の場合
            case XmlNodeType.Attribute:
                if (target is XAttribute attribute)
                {
                    attribute.Value = element.Value;
                }
                else
                {
                    throw new InvalidOperationException();
                }
                break;

            // それ以外の場合
            default:
                (target as XNode)?.ReplaceWith(element.Nodes());
                break;
        }
    }


    /// <summary>
    /// 要素を削除
    /// </summary>
    /// <param name="dst"></param>
    /// <param name="target"></param>
    private static void Remove(XDocument dst, XObject target)
    {
        switch (target.NodeType)
        {
            // 属性の場合
            case XmlNodeType.Attribute:
                (target as XAttribute)?.Remove();
                break;

            // それ以外の場合
            default:
                (target as XNode)?.Remove();
                break;
        }
    }
}
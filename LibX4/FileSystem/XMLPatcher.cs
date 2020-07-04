using System;
using System.Collections;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace X4_DataExporterWPF.Common
{
    /// <summary>
    /// XML差分用クラス
    /// </summary>
    /// <remarks>
    /// 詳細はRFC5261を参照
    /// </remarks>
    public class XMLPatcher
    {
        private const string BEFORE = "before";
        private const string AFTER = "after";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public XMLPatcher()
        {

        }


        /// <summary>
        /// xmlファイルをマージする
        /// </summary>
        /// <param name="patchedXml">パッチされるxml</param>
        /// <param name="patchXml">パッチ用xml</param>
        public void MergeXML(XDocument patchedXml, XDocument patchXml)
        {
            if (patchXml.Root.Name.LocalName == "diff")
            {
                var nsTbl = new NameTable();
                var nsMng = new XmlNamespaceManager(nsTbl);

                foreach (var attr in patchXml.Root.Attributes())
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

                foreach (var elm in patchXml.Root.Elements())
                {
                    var sel = patchedXml.XPathEvaluate(elm.Attribute("sel").Value, nsMng);

                    if (!(sel is IEnumerable enumerable))
                    {
                        continue;
                    }

                    var targetNode = enumerable.OfType<XObject>().FirstOrDefault();
                    if (targetNode == null)
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
                            Remove(patchedXml, targetNode);
                            break;

                        default:
                            break;
                    }
                }
            }
            else
            {
                patchedXml.Root.Add(patchXml.Root.Elements());
            }
        }





        /// <summary>
        /// 要素を追加
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="element"></param>
        /// <param name="target"></param>
        private void AddNode(XElement element, XObject target)
        {
            var type = element.Attribute("type")?.Value;


            //////////////////////
            // ノード追加の場合 //
            //////////////////////
            if (string.IsNullOrEmpty(type))
            {
                switch (element.Attribute("pos")?.Value ?? "prepend")
                {
                    case AFTER:
                        (target as XNode)?.AddAfterSelf(element.Nodes());
                        break;

                    case BEFORE:
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
        private void Replace(XElement element, XObject target)
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
        /// <param name="element"></param>
        private void Remove(XDocument dst, XObject target)
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
}
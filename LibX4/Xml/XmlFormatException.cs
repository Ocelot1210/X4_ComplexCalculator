using System;
using System.Xml.Linq;

namespace LibX4.Xml
{
    /// <summary>
    /// XML 文書の形式が無効である場合にスローされる例外
    /// </summary>
    public class XmlFormatException : FormatException
    {
        public XmlFormatException() { }

        public XmlFormatException(string message) : base(message) { }

        public XmlFormatException(string message, Exception? innerException)
            : base(message, innerException) { }


        /// <summary>
        /// 無効な値
        /// </summary>
        public string Input { get; private set; } = "<null>";


        /// <summary>
        /// 親要素
        /// </summary>
        public string Parent { get; private set; } = "<null>";


        /// <summary>
        /// 例外を説明するメッセージを取得
        /// </summary>
        public override string Message
            => string.Join(Environment.NewLine, base.Message,
                           $"Input: {Input}", $"Parent: {Parent}");


        /// <summary>
        /// XML 属性から XmlFormatException を初期化する
        /// </summary>
        /// <param name="attr">エラーの発生した XML 属性</param>
        /// <returns>指定の XML 属性の情報で初期化した XmlFormatException</returns>
        public static XmlFormatException CreateFrom(XAttribute? attr, Exception? innerException)
        {
            var exception = new XmlFormatException("XML format is invalid.", innerException);
            exception.Input = attr?.Value ?? "<null>";
            exception.Parent = attr?.Parent.ToString() ?? "<null>";
            return exception;
        }
    }
}

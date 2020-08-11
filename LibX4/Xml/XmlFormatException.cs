using System;

namespace LibX4.Xml
{
    /// <summary>
    /// XML 文書の形式が無効である場合にスローされる例外
    /// </summary>
    public class XmlFormatException : FormatException
    {
        /// <summary>
        /// 無効な値
        /// </summary>
        public string Input { get; }


        /// <summary>
        /// 例外を説明するメッセージを取得
        /// </summary>
        public override string Message => base.Message + $" (input: {Input})";


        public XmlFormatException(string? input)
        {
            Input = input ?? "<null>";
        }
    }
}

﻿using System;
using System.Globalization;
using System.Xml.Linq;

namespace LibX4.Xml;

/// <summary>
/// <see cref="XAttribute">XAttribute</see> の値を取得するための拡張メソッド郡
/// </summary>
public static class XAttributeExtension
{
    /// <summary>
    /// XML 属性の値を double として取得する
    /// </summary>
    /// <param name="attr">値を取得する XML 属性</param>
    /// <returns>変換済みの属性値</returns>
    /// <exception cref="XmlFormatException">属性が無い、または無効な値の場合</exception>
    public static double GetDouble(this XAttribute? attr)
    {
        try
        {
            var value = attr?.Value ?? throw XmlFormatException.CreateFrom(attr);
            return double.Parse(value, CultureInfo.InvariantCulture);
        }
        catch (SystemException exception)
        {
            throw XmlFormatException.CreateFrom(attr, exception);
        }
    }


    /// <summary>
    /// XML 属性の値を double として取得する。変換できない場合は default と見なす。
    /// </summary>
    /// <param name="attr">値を取得する XML 属性</param>
    /// <returns>変換済みの属性値又は 0.0</returns>
    public static double GetDoubleOrDefault(this XAttribute? attr)
    {
        try
        {
            var value = attr?.Value ?? throw XmlFormatException.CreateFrom(attr);
            return double.Parse(value, CultureInfo.InvariantCulture);
        }
        catch
        {
            return default;
        }
    }


    /// <summary>
    /// XML 属性の値を int として取得する
    /// </summary>
    /// <param name="attr">値を取得する XML 属性</param>
    /// <returns>変換済みの属性値</returns>
    /// <exception cref="XmlFormatException">属性が無い、または無効な値の場合</exception>
    public static int GetInt(this XAttribute? attr)
    {
        try
        {
            var value = attr?.Value ?? throw XmlFormatException.CreateFrom(attr);
            return int.Parse(value, CultureInfo.InvariantCulture);
        }
        catch (SystemException exception)
        {
            throw XmlFormatException.CreateFrom(attr, exception);
        }
    }
}

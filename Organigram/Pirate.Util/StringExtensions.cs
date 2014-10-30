using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace System
{
  public static class StringExtensions
  {
    public static bool IsNumeric(this string text)
    {
      int dummy;

      return int.TryParse(text, out dummy);
    }

    public static int ToNumber(this string text)
    {
      return Convert.ToInt32(text);
    }

    public static bool IsGreaterNumber(this string text, int value)
    {
      return text.IsNumeric() && text.ToNumber() > value;
    }

    public static bool IsGreaterOrEqualNumber(this string text, int value)
    {
      return text.IsNumeric() && text.ToNumber() >= value;
    }

    public static bool IsSmallerNumber(this string text, int value)
    {
      return text.IsNumeric() && text.ToNumber() < value;
    }

    public static bool IsSmallerOEqualNumber(this string text, int value)
    {
      return text.IsNumeric() && text.ToNumber() <= value;
    }

    public static bool IsInNumberRange(this string text, int start, int end)
    {
      return text.IsNumeric() && text.ToNumber() >= start && text.ToNumber() <= end;
    }
    
    public static bool IsNullOrEmpty(this string text)
    {
      return string.IsNullOrEmpty(text);
    }

    public static bool IsNotNullOrEmpty(this string text)
    {
      return !string.IsNullOrEmpty(text);
    }

    public static byte[] TryConvertFromBase64(this string value)
    {
      try
      {
        return Convert.FromBase64String(value);
      }
      catch
      {
        return null;
      }
    }

    public static string Head(this string text, string delimiter)
    {
      var index = text.IndexOf(delimiter);

      if (index >= 0)
      {
        return text.Substring(0, index);
      }
      else
      {
        return text;
      }
    }

    public static string Tail(this string text, string delimiter)
    {
      var index = text.IndexOf(delimiter);

      if (index >= 0)
      {
        return text.Substring(index + delimiter.Length);
      }
      else
      {
        return string.Empty;
      }
    }
  }
}
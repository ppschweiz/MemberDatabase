using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Util
{
  public class Base64Url
  {
    public static string To(byte[] data)
    {
      return
        Convert.ToBase64String(data)
        .Replace("+", "-")
        .Replace("/", "_")
        .Replace("=", ".");
    }

    public static byte[] From(string data)
    {
      return data
        .Replace("-", "+")
        .Replace("_", "/")
        .Replace(".", "=")
        .TryConvertFromBase64();
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace MemberAdmin
{
  public class QuotedPrintableConverter
  {
    private static string _Ascii7BitSigns;
    private const string _equalsSign = "=";
    private const string _defaultReplaceEqualSign = "=";

    /// <summary>
    /// Ctor.
    /// </summary>
    private QuotedPrintableConverter()
    {
      //do nothing
    }

    /// <summary>
    /// Encodes a not QP-Encoded string.
    /// </summary>
    /// <param name="value">The string which should be encoded.</param>
    /// <returns>The encoded string</returns>
    public static string Encode(string value)
    {
      return Encode(value, _defaultReplaceEqualSign);
    }

    /// <summary>
    /// Encodes a not QP-Encoded string.
    /// </summary>
    /// <param name="value">The string which should be encoded.</param>
    /// <param name="replaceEqualSign">The sign which should replace the "="-sign in front of 
    /// each QP-encoded sign.</param>
    /// <returns>The encoded string</returns>
    public static string Encode(string value, string replaceEqualSign)
    {
      //Alle nicht im Ascii-Zeichnsatz enthaltenen Zeichen werden ersetzt durch die hexadezimale 
      //Darstellung mit einem vorangestellten =
      //Bsp.: aus "ü" wird "=FC"
      //Bsp. mit Ersetzungszeichen "%"für das "=": aus "ü" wird "%FC"
      GetAllowedAsciiSigns();
      var sb = new StringBuilder();

      foreach (char s in value)
      {
        if (_Ascii7BitSigns.LastIndexOf(s) > -1)
        {
          sb.Append(s);
        }
        else
        {
          string qp = string.Format("{0}{1}",
              _equalsSign,
              System.Convert.ToString(s, 16)).Replace(_equalsSign, replaceEqualSign);
          sb.Append(qp);
        }
      }

      return sb.ToString();
    }

    /// <summary>
    /// Gets a string which contains the first 128-characters (ANSII 7 bit).
    /// </summary>
    private static void GetAllowedAsciiSigns()
    {
      var sb = new StringBuilder();

      for (int i = 0; i < 127; i++)
      {
        sb.Append(System.Convert.ToChar(i));
      }

      _Ascii7BitSigns = sb.ToString();
    }
  }
}
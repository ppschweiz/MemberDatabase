using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Util
{
  public static class Parsing
  {
    public static int TryParseInt32(string value, int defaultValue)
    {
      int result = defaultValue;
      int.TryParse(value, out result);
      return result;
    }

    public static DateTime? TryParseDateTime(string value)
    {
      DateTime result;

      if (DateTime.TryParseExact(value, System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern, System.Threading.Thread.CurrentThread.CurrentCulture, System.Globalization.DateTimeStyles.AssumeLocal, out result))
      {
        return result;
      }
      else
      {
        return null;
      }
    }
  }
}

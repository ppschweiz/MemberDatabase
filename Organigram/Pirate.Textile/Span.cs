using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pirate.Textile
{
  public static class Span
  {
    public static string RenderText(string text)
    {
      text = Regex.Replace(text, @"\*(.+?)\*", @"$1");
      text = Regex.Replace(text, @"_(.+?)_", @"$1");
      text = Regex.Replace(text, "\\\"(.+?)\\\"\\:(https?\\:\\S*)", @"$1 ( $2 )");
      return text;
    }

    public static string RenderHtml(string text)
    {
      text = Regex.Replace(text, @"\*(.+?)\*", @"<b>$1</b>");
      text = Regex.Replace(text, @"_(.+?)_", @"<i>$1</i>");
      text = Regex.Replace(text, "\\\"(.+?)\\\"\\:(https?\\:\\S*)", "<a href=\"$2\">$1</a>");
      return text;
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Pirate.Ldap.Organigram
{
  public static class ColorExtensions
  {
    public static string ToHtmlColor(this Color color)
    {
      return string.Format("{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
    }
  }
}

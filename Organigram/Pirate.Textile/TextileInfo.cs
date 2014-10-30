using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Textile
{
  public static class TextileInfo
  {
    public static string SupportedMarkup
    {
      get
      {
        return "h1. h2. h3. * list, # list, *bold* _italic_ \"text\":http://ab.ch";
      }
    }
  }
}

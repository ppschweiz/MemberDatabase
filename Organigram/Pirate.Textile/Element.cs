using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Textile
{
  public abstract class Element
  {
    public abstract string ToText();

    public abstract string ToHtml();
  }
}

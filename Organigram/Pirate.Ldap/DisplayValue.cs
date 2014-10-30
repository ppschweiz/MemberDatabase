using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  public class DisplayValue
  {
    public string Display { get; private set; }

    public string Value { get; private set; }

    public DisplayValue(string display, string value)
    {
      Display = display;
      Value = value;
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  public class Difference
  {
    public LdapAttributeBase Attribute { get; private set; }

    public string Before { get; private set; }

    public string After { get; private set; }

    public Difference(LdapAttributeBase attribute, string before, string after)
    {
      Attribute = attribute;
      Before = before;
      After = after;
    }
  }
}

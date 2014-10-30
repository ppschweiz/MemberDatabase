using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  public class FieldValue
  {
    public LdapAttributeBase Attribute { get; private set; }

    public string Value { get; private set; }

    public FieldValue(LdapAttributeBase attribute, string value)
    {
      Attribute = attribute;
      Value = value;
    }
  }
}

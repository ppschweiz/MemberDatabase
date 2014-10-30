using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;

namespace Pirate.Ldap
{
  public class LdapStringField : LdapField<string>
  {
    public LdapStringField(LdapAttributeBase attribute, Func<string> get, Action<string> set, bool required = false, bool identifying = false, bool defaultDisplay = false)
      : base(attribute, get, set, required, identifying, defaultDisplay)
    { 
    }

    public override bool IsModified(LdapField<string> original)
    {
      return Value != original.Value;
    }

    public override LdapModification GetModification(LdapField<string> original)
    {
      if (original.Value != Value)
      {
        if (string.IsNullOrEmpty(original.Value))
        {
          if (!string.IsNullOrEmpty(Value))
          {
            return new LdapModification(LdapModification.ADD, new LdapAttribute(Attribute, Value));
          }
          else
          {
            return null;
          }
        }
        else if (string.IsNullOrEmpty(Value))
        {
          return new LdapModification(LdapModification.DELETE, new LdapAttribute(Attribute));
        }
        else
        {
          return new LdapModification(LdapModification.REPLACE, new LdapAttribute(Attribute, Value));
        }
      }
      else
      {
        return null;
      }
    }

    public override string ValueString(MultiValueOutput output)
    {
      return Value;
    }

    public override IComparable CompareObject()
    {
      return Value;
    }

    public override bool IsNullOrEmpty()
    {
      return string.IsNullOrEmpty(Value);
    }

    public override void Load(LdapEntry entry)
    {
      Value = entry.GetAttributeStringValue(Attribute);
    }

    public override LdapAttribute GetValue()
    {
      if (!string.IsNullOrEmpty(Value))
      {
        return new LdapAttribute(Attribute, Value);
      }
      else
      {
        return null;
      }
    }
  }
}

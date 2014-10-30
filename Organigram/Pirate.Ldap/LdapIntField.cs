using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;

namespace Pirate.Ldap
{
  public class LdapIntField : LdapField<int?>
  {
    public LdapIntField(LdapAttributeBase attribute, Func<int?> get, Action<int?> set, bool required = false, bool identifying = false, bool defaultDisplay = false)
      : base(attribute, get, set, required, identifying, defaultDisplay)
    { 
    }

    public override bool IsModified(LdapField<int?> original)
    {
      return Value != original.Value;
    }

    public override LdapModification GetModification(LdapField<int?> original)
    {
      if (original.Value != Value)
      {
        if (!original.Value.HasValue)
        {
          return new LdapModification(LdapModification.ADD, new LdapAttribute(Attribute, Value.Value.ToString()));
        }
        else if (!Value.HasValue)
        {
          return new LdapModification(LdapModification.DELETE, new LdapAttribute(Attribute));
        }
        else
        {
          return new LdapModification(LdapModification.REPLACE, new LdapAttribute(Attribute, Value.Value.ToString()));
        }
      }
      else
      {
        return null;
      }
    }

    public override string ValueString(MultiValueOutput output)
    {
      return Value.ToString();
    }

    public override IComparable CompareObject()
    {
      return Value;
    }

    public override bool IsNullOrEmpty()
    {
      return !Value.HasValue;
    }

    public override void Load(LdapEntry entry)
    {
      Value = entry.GetAttributeIntegerValue(Attribute);
    }

    public override LdapAttribute GetValue()
    {
      if (Value.HasValue)
      {
        return new LdapAttribute(Attribute, Value.Value.ToString());
      }
      else
      {
        return null;
      }
    }
  }
}

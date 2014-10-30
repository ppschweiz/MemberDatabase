using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;
using System.Globalization;

namespace Pirate.Ldap
{
  public class LdapSbyteArrayField : LdapField<sbyte[]>
  {
    public LdapSbyteArrayField(LdapAttributeBase attribute, Func<sbyte[]> get, Action<sbyte[]> set, bool required = false, bool identifying = false, bool defaultDisplay = false)
      : base(attribute, get, set, required, identifying, defaultDisplay)
    { 
    }

    public override bool IsModified(LdapField<sbyte[]> original)
    {
      return !Value.AreEqual(original.Value);
    }

    public override LdapModification GetModification(LdapField<sbyte[]> original)
    {
      if (original.Value == null && Value == null)
      {
        return null;
      }
      else if (original.Value == null)
      {
        return new LdapModification(LdapModification.ADD, new LdapAttribute(Attribute, original.Value));
      }
      else if (Value == null)
      {
        return new LdapModification(LdapModification.DELETE, new LdapAttribute(Attribute));
      }
      else if (!Value.AreEqual(original.Value))
      {
        return new LdapModification(LdapModification.REPLACE, new LdapAttribute(Attribute, Value));
      }
      else
      {
        return null;
      }
    }

    public override string ValueString(MultiValueOutput output)
    {
      return Value.ToBytes().ToHexString();
    }

    public override IComparable CompareObject()
    {
      return Value.ToBytes().ToHexString();
    }

    public override bool IsNullOrEmpty()
    {
      return Value == null;
    }

    public override void Load(LdapEntry entry)
    {
      Value = entry.GetAttributeBytesValue(Attribute).ToSbytes();
    }

    public override LdapAttribute GetValue()
    {
      if (Value != null && Value.Length > 0)
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

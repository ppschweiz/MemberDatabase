using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;
using System.Globalization;

namespace Pirate.Ldap
{
  public class LdapByteArrayField : LdapField<byte[]>
  {
    public LdapByteArrayField(LdapAttributeBase attribute, Func<byte[]> get, Action<byte[]> set, bool required = false, bool identifying = false, bool defaultDisplay = false)
      : base(attribute, get, set, required, identifying, defaultDisplay)
    { 
    }

    public override bool IsModified(LdapField<byte[]> original)
    {
      return !Value.AreEqual(original.Value);
    }

    public override LdapModification GetModification(LdapField<byte[]> original)
    {
      if (original.Value == null && Value == null)
      {
        return null;
      }
      else if (original.Value == null)
      {
        return new LdapModification(LdapModification.ADD, new LdapAttribute(Attribute, original.Value.ToSbytes()));
      }
      else if (Value == null)
      {
        return new LdapModification(LdapModification.DELETE, new LdapAttribute(Attribute));
      }
      else if (!Value.AreEqual(original.Value))
      {
        return new LdapModification(LdapModification.REPLACE, new LdapAttribute(Attribute, Value.ToSbytes()));
      }
      else
      {
        return null;
      }
    }

    public override string ValueString(MultiValueOutput output)
    {
      return Value.ToHexString();
    }

    public override IComparable CompareObject()
    {
      return Value.ToHexString();
    }

    public override bool IsNullOrEmpty()
    {
      return Value == null;
    }

    public override void Load(LdapEntry entry)
    {
      Value = entry.GetAttributeBytesValue(Attribute);
    }

    public override LdapAttribute GetValue()
    {
      if (Value != null && Value.Length > 0)
      {
        return new LdapAttribute(Attribute, Value.ToSbytes());
      }
      else
      {
        return null;
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;
using System.Globalization;

namespace Pirate.Ldap
{
  public class LdapDataTimeField : LdapField<DateTime?>
  {
    public LdapDataTimeField(LdapAttributeBase attribute, Func<DateTime?> get, Action<DateTime?> set, bool required = false, bool identifying = false, bool defaultDisplay = false)
      : base(attribute, get, set, required, identifying, defaultDisplay)
    { 
    }

    public override bool IsModified(LdapField<DateTime?> original)
    {
      return Value != original.Value;
    }

    public override LdapModification GetModification(LdapField<DateTime?> original)
    {
      var oldValue = original.Value.HasValue ? original.Value.Value.ToString("yyyyMMddHHmmssZ", CultureInfo.InvariantCulture) : null;
      var newValue = Value.HasValue ? Value.Value.ToString("yyyyMMddHHmmssZ", CultureInfo.InvariantCulture) : null;

      if (oldValue != newValue)
      {
        if (string.IsNullOrEmpty(oldValue))
        {
          return new LdapModification(LdapModification.ADD, new LdapAttribute(Attribute, newValue));
        }
        else if (string.IsNullOrEmpty(newValue))
        {
          return new LdapModification(LdapModification.DELETE, new LdapAttribute(Attribute));
        }
        else
        {
          return new LdapModification(LdapModification.REPLACE, new LdapAttribute(Attribute, newValue));
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
      Value = entry.GetAttributeDateTimeValue(Attribute);
    }

    public override LdapAttribute GetValue()
    {
      if (Value.HasValue)
      {
        return new LdapAttribute(Attribute, Value.Value.ToString("yyyyMMddHHmmssZ", CultureInfo.InvariantCulture));
      }
      else
      {
        return null;
      }
    }
  }
}

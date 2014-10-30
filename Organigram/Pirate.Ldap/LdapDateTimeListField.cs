using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;
using System.Globalization;

namespace Pirate.Ldap
{
  public class LdapDateTimeListField : LdapField<IEnumerable<DateTime>>
  {
    public LdapDateTimeListField(LdapAttributeBase attribute, Func<IEnumerable<DateTime>> get, Action<IEnumerable<DateTime>> set, bool required = false, bool identifying = false, bool defaultDisplay = false)
      : base(attribute, get, set, required, identifying, defaultDisplay)
    { 
    }

    public override bool IsModified(LdapField<IEnumerable<DateTime>> original)
    {
      return !Value.AreEqual(original.Value);
    }

    public override LdapModification GetModification(LdapField<IEnumerable<DateTime>> original)
    {
      if (Value.Count() == 0 && original.Value.Count() == 0)
      {
        return null;
      }
      else if (original.Value.Count() == 0)
      {
        return new LdapModification(LdapModification.ADD, new LdapAttribute(Attribute, Value.Select(x => x.ToString("yyyyMMddHHmmssZ", CultureInfo.InvariantCulture)).ToArray()));
      }
      else if (Value.Count() == 0)
      {
        return new LdapModification(LdapModification.DELETE, new LdapAttribute(Attribute));
      }
      else if (!Value.AreEqual(original.Value))
      {
        return new LdapModification(LdapModification.REPLACE, new LdapAttribute(Attribute, Value.Select(x => x.ToString("yyyyMMddHHmmssZ", CultureInfo.InvariantCulture)).ToArray()));
      }
      else
      {
        return null;
      }
    }

    public override string ValueString(MultiValueOutput output)
    {
      return string.Join(Environment.NewLine, Value.Select(x => x.ToString()).ToArray());
    }

    public override IComparable CompareObject()
    {
      return new CompareList<DateTime>(Value);
    }

    public override bool IsNullOrEmpty()
    {
      return Value.Count() == 0;
    }

    public override void Load(LdapEntry entry)
    {
      Value = entry.GetAttributeDateTimeValues(Attribute);
    }

    public override LdapAttribute GetValue()
    {
      if (Value.Count() > 0)
      {
        return new LdapAttribute(Attribute, Value.Select(x => x.ToString("yyyyMMddHHmmssZ", CultureInfo.InvariantCulture)).ToArray());
      }
      else 
      {
        return null;
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;
using System.Globalization;

namespace Pirate.Ldap
{
  public class LdapStringListField : LdapField<IEnumerable<string>>
  {
    public LdapStringListField(LdapAttributeBase attribute, Func<IEnumerable<string>> get, Action<IEnumerable<string>> set, bool required = false, bool identifying = false, bool defaultDisplay = false)
      : base(attribute, get, set, required, identifying, defaultDisplay)
    { 
    }

    public override bool IsModified(LdapField<IEnumerable<string>> original)
    {
      return !Value.AreEqual(original.Value);
    }

    public override LdapModification GetModification(LdapField<IEnumerable<string>> original)
    {
      if (Value.Count() == 0 && original.Value.Count() == 0)
      {
        return null;
      }
      else if (original.Value.Count() == 0)
      {
        return new LdapModification(LdapModification.ADD, new LdapAttribute(Attribute, Value.ToArray()));
      }
      else if (Value.Count() == 0)
      {
        return new LdapModification(LdapModification.DELETE, new LdapAttribute(Attribute));
      }
      else if (!Value.AreEqual(original.Value))
      {
        return new LdapModification(LdapModification.REPLACE, new LdapAttribute(Attribute, Value.ToArray()));
      }
      else
      {
        return null;
      }
    }

    public override string ValueString(MultiValueOutput output)
    {
      switch (output)
      {
        case MultiValueOutput.MultiLine:
          return string.Join(Environment.NewLine, Value.ToArray());
        case MultiValueOutput.Separated:
          return string.Join(", ", Value.ToArray());
        case MultiValueOutput.FirstOnly:
          return Value.FirstOrDefault();
        default:
          throw new ArgumentOutOfRangeException("output");
      }
    }

    public override IComparable CompareObject()
    {
      return new CompareList<string>(Value);
    }

    public override bool IsNullOrEmpty()
    {
      return Value.Count() == 0;
    }

    public override void Load(LdapEntry entry)
    {
      Value = entry.GetAttributeStringValues(Attribute);
    }

    public override LdapAttribute GetValue()
    {
      if (Value.Count() > 0)
      {
        return new LdapAttribute(Attribute, Value.ToArray());
      }
      else
      {
        return null;
      }
    }
  }
}

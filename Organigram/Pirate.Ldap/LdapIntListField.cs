using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;
using System.Globalization;

namespace Pirate.Ldap
{
  public class LdapIntListField : LdapField<IEnumerable<int>>
  {
    public LdapIntListField(LdapAttributeBase attribute, Func<IEnumerable<int>> get, Action<IEnumerable<int>> set, bool required = false, bool identifying = false, bool defaultDisplay = false)
      : base(attribute, get, set, required, identifying, defaultDisplay)
    { 
    }

    public override bool IsModified(LdapField<IEnumerable<int>> original)
    {
      return !Value.AreEqual(original.Value);
    }

    public override LdapModification GetModification(LdapField<IEnumerable<int>> original)
    {
      if (Value.Count() == 0 && original.Value.Count() == 0)
      {
        return null;
      }
      else if (original.Value.Count() == 0)
      {
        return new LdapModification(LdapModification.ADD, new LdapAttribute(Attribute, Value.Select(x => x.ToString()).ToArray()));
      }
      else if (Value.Count() == 0)
      {
        return new LdapModification(LdapModification.DELETE, new LdapAttribute(Attribute));
      }
      else if (!Value.AreEqual(original.Value))
      {
        return new LdapModification(LdapModification.REPLACE, new LdapAttribute(Attribute, Value.Select(x => x.ToString()).ToArray()));
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
          return string.Join(Environment.NewLine, Value.Select(x => x.ToString()).ToArray());
        case MultiValueOutput.Separated:
          return string.Join(", ", Value.Select(x => x.ToString()).ToArray());
        case MultiValueOutput.FirstOnly:
          return Value.Select(x => x.ToString()).FirstOrDefault();
        default:
          throw new ArgumentOutOfRangeException("output");
      } 
    }

    public override IComparable CompareObject()
    {
      return new CompareList<int>(Value);
    }

    public override bool IsNullOrEmpty()
    {
      return Value.Count() == 0;
    }

    public override void Load(LdapEntry entry)
    {
      Value = entry.GetAttributeIntegerValues(Attribute);
    }

    public override LdapAttribute GetValue()
    {
      if (Value.Count() > 0)
      {
        return new LdapAttribute(Attribute, Value.Select(x => x.ToString()).ToArray());
      }
      else
      {
        return null;
      }
    }
  }
}

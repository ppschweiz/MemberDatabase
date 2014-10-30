using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;

namespace Pirate.Ldap
{
  public abstract class LdapField : Field
  {
    public bool Required { get; private set; }

    public abstract bool IsNullOrEmpty();

    public abstract void Load(LdapEntry entry);

    public abstract LdapModification GetModification(LdapObject originalObject);

    public abstract Difference GetDifference(LdapObject originalObject, MultiValueOutput output);

    public abstract LdapAttribute GetValue();

    public LdapField(LdapAttributeBase attribute, bool required, bool identifying, bool defaultDisplay)
      : base(attribute, identifying, defaultDisplay)
    {
      Required = required;
    }
  }

  public abstract class LdapField<TValue> : LdapField
  {
    private Func<TValue> _get;

    private Action<TValue> _set;

    public LdapField(LdapAttributeBase attribute, Func<TValue> get, Action<TValue> set, bool required, bool identifying, bool defaultDisplay)
      : base(attribute, required, identifying, defaultDisplay)
    {
      _get = get;
      _set = set;
    }

    public TValue Value
    {
      get { return _get(); }
      set { _set(value); }
    }

    public abstract bool IsModified(LdapField<TValue> original);

    public abstract LdapModification GetModification(LdapField<TValue> original);

    public override LdapModification GetModification(LdapObject originalObject)
    {
      return GetModification(originalObject.GetField<TValue>(Attribute));
    }

    public override Difference GetDifference(LdapObject originalObject, MultiValueOutput output)
    {
      var original = originalObject.GetField<TValue>(Attribute);

      if (original.IsNullOrEmpty() && IsNullOrEmpty())
      {
        return null;
      }
      else if (original.IsNullOrEmpty())
      {
        return new Difference(Attribute, null, ValueString(output));
      }
      else if (IsNullOrEmpty())
      {
        return new Difference(Attribute, original.ValueString(output), null);
      }
      else if (IsModified(original))
      {
        return new Difference(Attribute, original.ValueString(output), ValueString(output));
      }
      else
      {
        return null;
      }
    }
  }
}

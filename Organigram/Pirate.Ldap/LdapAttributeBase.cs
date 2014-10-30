using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  public class LdapAttributeBase
  {
    public const string AnyChar = "*";

    private readonly string _value;

    private readonly Func<string> _translation;

    public string Text()
    {
      return _translation();
    }

    public LdapAttributeBase(string value, Func<string> translation)
    {
      _value = value;
      _translation = translation;
    }

    public LdapFilter AnyOf(IEnumerable<EmployeeType> values)
    {
      return AnyOf(values.Select(v => (int)v));
    }

    public LdapFilter AnyOf(params EmployeeType[] values)
    {
      return AnyOf(values.Select(v => (int)v));
    }

    public LdapFilter AnyOf(params int[] values)
    {
      return AnyOf((IEnumerable<int>)values);
    }

    public LdapFilter AnyOf(IEnumerable<int> values)
    {
      LdapFilter filter = null;

      foreach (var value in values)
      {
        if (filter == null)
        {
          filter = this == value;
        }
        else
        {
          filter |= (this == value);
        }
      }

      return filter;
    }

    public LdapFilter AnyOf(params string[] values)
    {
      return AnyOf((IEnumerable<string>)values);
    }

    public LdapFilter AnyOf(IEnumerable<string> values)
    {
      LdapFilter filter = null;

      foreach (var value in values)
      {
        var newFilter = string.IsNullOrEmpty(value) ? this.Null : this == value;
        filter = filter == null ? newFilter : filter | newFilter;
      }

      return filter;
    }

    public LdapFilter Any
    {
      get
      {
        return this == AnyChar;
      }
    }

    public LdapFilter Null
    {
      get
      {
        return this != AnyChar;
      }
    }

    public static implicit operator string(LdapAttributeBase attribute)
    {
      return attribute._value;
    }

    public static LdapFilter operator ==(LdapAttributeBase attribute, string value)
    {
      return new LdapAttributeFilter(attribute, value);
    }

    public static LdapFilter operator ==(LdapAttributeBase attribute, int value)
    {
      return new LdapAttributeFilter(attribute, value);
    }

    public static LdapFilter operator !=(LdapAttributeBase attribute, string value)
    {
      return !(attribute == value);
    }

    public static LdapFilter operator !=(LdapAttributeBase attribute, int value)
    {
      return !(attribute == value);
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;

namespace Pirate.Ldap
{
  public class LdapEnumField<TEnum> : LdapIntField
    where TEnum : struct, IConvertible
  {
    private TEnum _defaultValue;
    private Func<TEnum, string> _valueTranslation;

    public LdapEnumField(LdapAttributeBase attribute, Func<int?> get, Action<int?> set, TEnum defaultValue, Func<TEnum, string> valueTranslation, bool required = false, bool identifying = false)
      : base(attribute, get, set, required, identifying)
    {
      _defaultValue = defaultValue;
      _valueTranslation = valueTranslation;
    }

    public TEnum EnumValue
    {
      get
      {
        if (Value.HasValue)
        {
          return (TEnum)(object)Value.Value;
        }
        else
        {
          return _defaultValue;
        }
      }
      set
      {
        Value = (int)(object)value;
      }
    }

    public override string ValueString(MultiValueOutput output)
    {
      return _valueTranslation(EnumValue);
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;
using System.Globalization;

namespace Pirate.Ldap
{
  public class LdapEnumListField<TEnum> : LdapIntListField
        where TEnum : struct, IConvertible
  {
    public LdapEnumListField(LdapAttributeBase attribute, Func<IEnumerable<int>> get, Action<IEnumerable<int>> set, bool required = false, bool identifying = false)
      : base(attribute, get, set, required, identifying)
    { 
    }

    public IEnumerable<TEnum> EnumValues
    {
      get
      {
        return Value.Select(v => (TEnum)(object)v);
      }
      set
      {
        Value = value.Select(v => (int)(object)v);
      }
    }

    public override string ValueString(MultiValueOutput output)
    {
      return string.Join(", ", EnumValues.Select(v => v.ToString()).ToArray());
    }
  }
}

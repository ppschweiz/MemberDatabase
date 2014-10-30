using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  public class ComputedField : Field
  {
    private readonly Func<string> _compute;

    public ComputedField(LdapAttributeBase attribute, bool identifying, bool defaultDisplay, Func<string> compute)
      : base(attribute, identifying, defaultDisplay)
    {
      _compute = compute;
    }

    public override string ValueString(MultiValueOutput output)
    {
      return _compute();
    }

    public override IComparable CompareObject()
    {
      return _compute();
    }
  }
}

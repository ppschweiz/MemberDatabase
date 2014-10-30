using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;

namespace Pirate.Ldap
{
  public class EvaulateField : Field
  {
    private readonly Func<LdapConnection, string> _compute;

    public EvaulateField(LdapAttributeBase attribute, bool identifying, bool defaultDisplay, Func<LdapConnection, string> compute)
      : base(attribute, identifying, defaultDisplay)
    {
      _compute = compute;
    }

    public override string ValueString(MultiValueOutput output)
    {
      return null;
    }

    public override IComparable CompareObject()
    {
      return null;
    }

    public override string EvaluateString(LdapConnection connection)
    {
      return _compute(connection);
    }

    public override bool NeedsEvaluation
    {
      get
      {
        return true;
      }
    }
  }
}

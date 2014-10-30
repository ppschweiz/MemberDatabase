using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  /// <summary>
  /// Negates an LDAP filter.
  /// </summary>
  public class LdapNotFilter : LdapFilter
  {
    /// <summary>
    /// Gets the ldap filter that is negated.
    /// </summary>
    public LdapFilter Sub { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LdapNotFilter"/> class.
    /// </summary>
    /// <param name="sub">The sub.</param>
    public LdapNotFilter(LdapFilter sub)
    {
      Sub = sub;
    }

    /// <summary>
    /// Gets the filter text.
    /// </summary>
    public override string Text
    {
      get { return "(!" + Sub.Text + ")"; }
    }
  }
}

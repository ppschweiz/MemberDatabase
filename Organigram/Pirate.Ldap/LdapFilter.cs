using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  /// <summary>
  /// Filter for LDAP.
  /// </summary>
  public abstract class LdapFilter
  {
    /// <summary>
    /// Gets the filter text.
    /// </summary>
    public abstract string Text { get; }

    public static LdapFilter operator &(LdapFilter a, LdapFilter b)
    {
      if (a == null && b == null)
      {
        return null;
      }
      else if (a == null)
      {
        return b;
      }
      else if (b == null)
      {
        return a;
      }
      else
      {
        return new LdapAndFilter(a, b);
      }
    }

    public static LdapFilter operator |(LdapFilter a, LdapFilter b)
    {
      if (a == null && b == null)
      {
        return null;
      }
      else if (a == null)
      {
        return b;
      }
      else if (b == null)
      {
        return a;
      }
      else
      {
        return new LdapOrFilter(a, b);
      }
    }

    public static LdapFilter operator !(LdapFilter a)
    {
      if (a == null)
      {
        return null;
      }
      else
      {
        return new LdapNotFilter(a);
      }
    }
  }
}

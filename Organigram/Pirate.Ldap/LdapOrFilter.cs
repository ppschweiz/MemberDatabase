using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  /// <summary>
  /// Disjunction between to LDAP filters.
  /// </summary>
  public class LdapOrFilter : LdapFilter
  {
    /// <summary>
    /// Gets the right filter.
    /// </summary>
    public LdapFilter Right { get; private set; }

    /// <summary>
    /// Gets the left filter.
    /// </summary>
    public LdapFilter Left { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LdapOrFilter"/> class.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    public LdapOrFilter(LdapFilter left, LdapFilter right)
    {
      Right = right;
      Left = left;
    }

    /// <summary>
    /// Gets the filter text.
    /// </summary>
    public override string Text
    {
      get { return "(|" + Right.Text + Left.Text + ")"; }
    }

    public static LdapFilter Multiple(IEnumerable<LdapFilter> filters)
    {
      LdapFilter master = null;

      foreach (var filter in filters)
      {
        if (master == null)
        {
          master = filter;
        }
        else
        {
          master = new LdapOrFilter(master, filter);
        }
      }

      return master;
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  public class LdapAndFilter : LdapFilter
  {
    public LdapFilter Right { get; private set; }

    public LdapFilter Left { get; private set; }

    public LdapAndFilter(LdapFilter left, LdapFilter right)
    {
      Right = right;
      Left = left;
    }

    public override string Text
    {
      get { return "(&" + Right.Text + Left.Text + ")"; }
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
          master = new LdapAndFilter(master, filter);
        }
      }

      return master;
    }
  }
}

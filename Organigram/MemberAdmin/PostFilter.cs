using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pirate.Ldap;

namespace MemberAdmin
{
  public class PostFilter
  {
    public Func<LdapObject, bool> Function { get; private set; }

    public string Description { get; private set; }

    public PostFilter(string description, Func<LdapObject, bool> function)
    {
      Description = description;
      Function = function;
    }
  }
}
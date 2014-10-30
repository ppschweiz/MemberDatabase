using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  /// <summary>
  /// LDAP query scope.
  /// </summary>
  public enum LdapScope
  {
    Base = 0,
    One = 1,
    Sub = 2
  }
}

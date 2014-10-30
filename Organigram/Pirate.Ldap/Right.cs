using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  [Flags]
  public enum Right
  {
    None = 0,
    Read = 1,
    Write = 2,
    ReadWrite = 3,
    Manage = 7
  }
}
